using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Sentinel.Core.Contracts.Events;
using Sentinel.Core.Contracts.Telemetry;
using Sentinel.Core.Enums;
using Sentinel.Satellite.Service.Constants;
using Sentinel.Satellite.Service.Contracts.OnboardAi;
using Sentinel.Satellite.Service.Jobs.SatelliteHealthCheck.Options;
using Sentinel.Satellite.Service.Options.Satellite;
using Sentinel.Satellite.Service.Services;
using Sentinel.Satellite.Service.Services.Clients.GroundApi;
using Sentinel.Satellite.Service.Services.DecisionEngine;
using Sentinel.Satellite.Service.Services.TelemetrySimulator;

namespace Sentinel.Satellite.Service.Jobs.SatelliteHealthCheck;

public sealed class SatelliteHealthCheckJob(
    [FromKeyedServices(DataSources.Ground)] NpgsqlDataSource groundDb,
    TelemetryBucketReader bucketReader,
    IOnboardAiClient onboardAi,
    TelemetrySimulatorService simulator,
    DecisionEngine decisionEngine,
    IGroundApiClient groundApi,
    IOptions<SatelliteOptions> satelliteOptions,
    ILogger<SatelliteHealthCheckJob> logger,
    IOptions<SatelliteHealthCheckOptions> options) : BackgroundService
{
    private const string SqlStatusActive = nameof(SatelliteStatus.Active);
    private const string SqlLinkStatusOnline = nameof(LinkStatus.Online);

    private const string SqlSelectSatellites = """
        SELECT id, mission_id, name, norad_id, external_id, status, created_at
        FROM satellites
        WHERE status = @status
        """;

    private const string SqlInsertMlHealth = """
        INSERT INTO ml_health_results (id, satellite_id, bucket_start, model_name, model_version, anomaly_score, confidence, per_signal_score, top_contributors, created_at)
        VALUES (@id, @satellite_id, @bucket_start, @model_name, @model_version, @anomaly_score, @confidence, CAST(@per_signal_score AS jsonb), CAST(@top_contributors AS jsonb), @created_at)
        """;

    private const string SqlInsertDecision = """
        INSERT INTO decisions (id, satellite_id, bucket_start, decision_type, reason, created_at)
        VALUES (@id, @satellite_id, @bucket_start, @decision_type, @reason, @created_at)
        """;

    private const string SqlUpdateSatelliteLinkStatus = """
        UPDATE satellites SET link_status = @link_status WHERE id = @id
        """;

    private readonly NpgsqlDataSource groundDb = groundDb;
    private readonly TelemetryBucketReader bucketReader = bucketReader;
    private readonly IOnboardAiClient onboardAi = onboardAi;
    private readonly TelemetrySimulatorService simulator = simulator;
    private readonly DecisionEngine decisionEngine = decisionEngine;
    private readonly IGroundApiClient groundApi = groundApi;
    private readonly IOptions<SatelliteOptions> satelliteOptions = satelliteOptions;
    private readonly ILogger<SatelliteHealthCheckJob> logger = logger;
    private readonly TimeSpan interval = options.Value.Interval;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var instanceSatelliteId = satelliteOptions.Value.Id;
                await RunAsync(instanceSatelliteId, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Satellite health check cycle failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    public async Task RunAsync(Guid? satelliteId, CancellationToken cancellationToken)
    {
        await using var conn = await groundDb.OpenConnectionAsync(cancellationToken);
        var satellites = (await conn.QueryAsync<SatelliteRow>(
            new CommandDefinition(SqlSelectSatellites, new { status = SqlStatusActive }, cancellationToken: cancellationToken))).ToList();
        if (satelliteId.HasValue) satellites = satellites.Where(s => s.Id == satelliteId.Value).ToList();

        var useOnboardSimulate = simulator.Running;

        foreach (var sat in satellites)
        {
            try
            {
                TelemetryHealthResponse? mlResponse;
                if (useOnboardSimulate)
                {
                    mlResponse = await onboardAi.SimulateScoreAsync(
                        new OnboardAiSimulateRequest { Scenario = simulator.CurrentScenario.ToString() },
                        cancellationToken);
                }
                else
                {
                    var bucket = await bucketReader.GetLatestBucketAsync(sat.Id, cancellationToken);
                    if (bucket is null) continue;

                    var scoreRequest = OnboardAiMapper.ToScoreRequest(bucket);
                    mlResponse = await onboardAi.ScoreAsync(scoreRequest, cancellationToken);
                }

                if (mlResponse == null) continue;

                var bucketStart = DateTimeOffset.Parse(mlResponse.BucketStart, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var mlResultId = Guid.NewGuid();
                await conn.ExecuteAsync(new CommandDefinition(SqlInsertMlHealth, new
                {
                    id = mlResultId,
                    satellite_id = sat.Id,
                    bucket_start = bucketStart,
                    model_name = mlResponse.Ml.Model.Name,
                    model_version = mlResponse.Ml.Model.Version,
                    anomaly_score = mlResponse.Ml.AnomalyScore,
                    confidence = mlResponse.Ml.Confidence,
                    per_signal_score = JsonSerializer.Serialize(mlResponse.Ml.PerSignalScore),
                    top_contributors = JsonSerializer.Serialize(mlResponse.Ml.TopContributors.Select(c => new { key = c.Key, weight = c.Weight })),
                    created_at = DateTimeOffset.UtcNow
                }, cancellationToken: cancellationToken));

                var (decisionType, reason) = decisionEngine.Decide(mlResponse);
                var decisionId = Guid.NewGuid();
                await conn.ExecuteAsync(new CommandDefinition(SqlInsertDecision, new
                {
                    id = decisionId,
                    satellite_id = sat.Id,
                    bucket_start = bucketStart,
                    decision_type = decisionType.ToString(),
                    reason,
                    created_at = DateTimeOffset.UtcNow
                }, cancellationToken: cancellationToken));

                await groundApi.PublishAsync(
                    new GroundEventContract
                    {
                        EventId = Guid.NewGuid(),
                        MissionId = sat.MissionId,
                        SatelliteId = sat.Id,
                        Type = "healthcheck",
                        Ts = DateTimeOffset.UtcNow,
                        BucketStart = bucketStart,
                        Payload = new
                        {
                            mlResult = mlResponse.Ml,
                            decision = new { type = decisionType.ToString(), reason }
                        }
                    }, cancellationToken);

                await conn.ExecuteAsync(new CommandDefinition(
                    SqlUpdateSatelliteLinkStatus,
                    new { id = sat.Id, link_status = SqlLinkStatusOnline },
                    cancellationToken: cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Health check failed for satellite {SatelliteId}", sat.Id);
            }
        }
    }

    private sealed class SatelliteRow
    {
        public Guid Id { get; init; }

        public Guid? MissionId { get; init; }

        public string Name { get; init; } = string.Empty;

        public int? NoradId { get; init; }

        public string? ExternalId { get; init; }

        public string Status { get; init; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; }
    }
}

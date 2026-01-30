using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Satellite.Service.Contracts.OnboardAi;
using Sentinel.Satellite.Service.Options;
using Sentinel.Satellite.Service.Services;
using System.Text.Json;

namespace Sentinel.Satellite.Service.Jobs;

public sealed class SatelliteHealthCheckJob : BackgroundService
{
    private readonly NpgsqlDataSource _groundDb;
    private readonly TelemetryBucketReader _bucketReader;
    private readonly IOnboardAiClient _onboardAi;
    private readonly DecisionEngine _decisionEngine;
    private readonly GroundEventsClient _eventsClient;
    private readonly IOptions<SatelliteOptions> _satelliteOptions;
    private readonly ILogger<SatelliteHealthCheckJob> _logger;
    private readonly TimeSpan _interval;

    public SatelliteHealthCheckJob(
        [FromKeyedServices("ground")] NpgsqlDataSource groundDb,
        TelemetryBucketReader bucketReader,
        IOnboardAiClient onboardAi,
        DecisionEngine decisionEngine,
        GroundEventsClient eventsClient,
        IOptions<SatelliteOptions> satelliteOptions,
        ILogger<SatelliteHealthCheckJob> logger,
        IOptions<SatelliteHealthCheckOptions> options)
    {
        _groundDb = groundDb;
        _bucketReader = bucketReader;
        _onboardAi = onboardAi;
        _decisionEngine = decisionEngine;
        _eventsClient = eventsClient;
        _satelliteOptions = satelliteOptions;
        _logger = logger;
        _interval = options.Value.Interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var instanceSatelliteId = _satelliteOptions.Value.Id;
                await RunAsync(instanceSatelliteId, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Satellite health check cycle failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    public async Task RunAsync(Guid? satelliteId, CancellationToken cancellationToken)
    {
        await using var conn = await _groundDb.OpenConnectionAsync(cancellationToken);
        const string sql = "SELECT id, mission_id, name, norad_id, external_id, status, created_at FROM satellites WHERE status = @status";
        var satellites = (await conn.QueryAsync<SatelliteRow>(new CommandDefinition(sql, new { status = nameof(SatelliteStatus.Active) }, cancellationToken: cancellationToken))).ToList();
        if (satelliteId.HasValue)
            satellites = satellites.Where(s => s.Id == satelliteId.Value).ToList();

        foreach (var sat in satellites)
        {
            try
            {
                var bucket = await _bucketReader.GetLatestBucketAsync(sat.Id, cancellationToken);
                if (bucket == null) continue;

                var scoreRequest = OnboardAiMapper.ToScoreRequest(bucket);
                var mlResponse = await _onboardAi.ScoreAsync(scoreRequest, cancellationToken);
                if (mlResponse == null) continue;

                var bucketStart = DateTime.Parse(mlResponse.BucketStart, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var mlResultId = Guid.NewGuid();
                const string insertMl = """
                    INSERT INTO ml_health_results (id, satellite_id, bucket_start, model_name, model_version, anomaly_score, confidence, per_signal_score, top_contributors, created_at)
                    VALUES (@id, @satellite_id, @bucket_start, @model_name, @model_version, @anomaly_score, @confidence, @per_signal_score, @top_contributors, @created_at)
                    """;
                await conn.ExecuteAsync(new CommandDefinition(insertMl, new
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
                    created_at = DateTime.UtcNow
                }, cancellationToken: cancellationToken));

                var (decisionType, reason) = _decisionEngine.Decide(mlResponse);
                var decisionId = Guid.NewGuid();
                const string insertDecision = """
                    INSERT INTO decisions (id, satellite_id, bucket_start, decision_type, reason, created_at)
                    VALUES (@id, @satellite_id, @bucket_start, @decision_type, @reason, @created_at)
                    """;
                await conn.ExecuteAsync(new CommandDefinition(insertDecision, new
                {
                    id = decisionId,
                    satellite_id = sat.Id,
                    bucket_start = bucketStart,
                    decision_type = decisionType.ToString(),
                    reason,
                    created_at = DateTime.UtcNow
                }, cancellationToken: cancellationToken));

                await _eventsClient.PublishAsync(new GroundEventContract
                {
                    Type = "ml_result",
                    SatelliteId = sat.Id,
                    BucketStart = mlResponse.BucketStart,
                    Payload = mlResponse.Ml
                }, cancellationToken);
                await _eventsClient.PublishAsync(new GroundEventContract
                {
                    Type = "decision",
                    SatelliteId = sat.Id,
                    BucketStart = mlResponse.BucketStart,
                    Payload = new { type = decisionType.ToString(), reason }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for satellite {SatelliteId}", sat.Id);
            }
        }
    }

    private sealed class SatelliteRow
    {
        public Guid Id { get; init; }
        public Guid MissionId { get; init; }
        public string Name { get; init; } = "";
        public int? NoradId { get; init; }
        public string? ExternalId { get; init; }
        public string Status { get; init; } = "";
        public DateTime CreatedAt { get; init; }
    }
}

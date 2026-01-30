using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Services;
using System.Text.Json;

namespace Sentinel.Ground.Api.Jobs;

public sealed class SatelliteHealthCheckJob
{
    private readonly TelemetryBucketReader _bucketReader;
    private readonly MlClient _mlClient;
    private readonly DecisionEngine _decisionEngine;
    private readonly MlResultRepository _mlResultRepo;
    private readonly DecisionRepository _decisionRepo;
    private readonly SatelliteRepository _satelliteRepo;
    private readonly SseEventBus _eventBus;
    private readonly ILogger<SatelliteHealthCheckJob> _logger;

    public SatelliteHealthCheckJob(
        TelemetryBucketReader bucketReader,
        MlClient mlClient,
        DecisionEngine decisionEngine,
        MlResultRepository mlResultRepo,
        DecisionRepository decisionRepo,
        SatelliteRepository satelliteRepo,
        SseEventBus eventBus,
        ILogger<SatelliteHealthCheckJob> logger)
    {
        _bucketReader = bucketReader;
        _mlClient = mlClient;
        _decisionEngine = decisionEngine;
        _mlResultRepo = mlResultRepo;
        _decisionRepo = decisionRepo;
        _satelliteRepo = satelliteRepo;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task RunAsync(Guid? satelliteId = null, CancellationToken cancellationToken = default)
    {
        var satellites = await _satelliteRepo.GetActiveAsync(cancellationToken);
        if (satelliteId.HasValue)
            satellites = satellites.Where(s => s.Id == satelliteId.Value).ToList();
        foreach (var sat in satellites)
        {
            try
            {
                var bucket = await _bucketReader.GetLatestBucketAsync(sat.Id, cancellationToken);
                if (bucket == null) continue;

                var mlResponse = await _mlClient.EvaluateAsync(bucket, cancellationToken);
                if (mlResponse == null) continue;

                var bucketStart = DateTime.Parse(mlResponse.BucketStart, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var mlResult = new MlHealthResult
                {
                    SatelliteId = sat.Id,
                    BucketStart = bucketStart,
                    ModelName = mlResponse.Ml.Model.Name,
                    ModelVersion = mlResponse.Ml.Model.Version,
                    AnomalyScore = mlResponse.Ml.AnomalyScore,
                    Confidence = mlResponse.Ml.Confidence,
                    PerSignalScore = JsonSerializer.Serialize(mlResponse.Ml.PerSignalScore),
                    TopContributors = JsonSerializer.Serialize(mlResponse.Ml.TopContributors.Select(c => new { key = c.Key, weight = c.Weight })),
                    CreatedAt = DateTime.UtcNow
                };
                await _mlResultRepo.InsertAsync(mlResult, cancellationToken);

                var (decisionType, reason) = _decisionEngine.Decide(mlResponse);
                var decision = new Decision
                {
                    SatelliteId = sat.Id,
                    BucketStart = bucketStart,
                    DecisionType = decisionType,
                    Reason = reason,
                    CreatedAt = DateTime.UtcNow
                };
                await _decisionRepo.InsertAsync(decision, cancellationToken);

                await _eventBus.PublishAsync(new GroundEventContract
                {
                    Type = "ml_result",
                    SatelliteId = sat.Id,
                    BucketStart = mlResponse.BucketStart,
                    Payload = mlResponse.Ml
                }, cancellationToken);
                await _eventBus.PublishAsync(new GroundEventContract
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
}

using Sentinel.Core.Contracts.Telemetry;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public static class OnboardAiMapper
{
    public static OnboardAiScoreRequest ToScoreRequest(TelemetryBucketRequest bucket)
    {
        var features = new Dictionary<string, SignalFeaturesDto>();
        foreach (var kv in bucket.Signals)
        {
            features[kv.Key] = new SignalFeaturesDto
            {
                Mean = kv.Value.Mean,
                Min = kv.Value.Min,
                Max = kv.Value.Max,
                Std = kv.Value.Std,
                Slope = kv.Value.Slope,
                P95 = kv.Value.P95,
                MissingRate = kv.Value.MissingRate
            };
        }

        return new OnboardAiScoreRequest
        {
            SchemaVersion = bucket.SchemaVersion,
            BucketStart = bucket.BucketStart,
            BucketSec = bucket.BucketSec,
            Features = features
        };
    }
}

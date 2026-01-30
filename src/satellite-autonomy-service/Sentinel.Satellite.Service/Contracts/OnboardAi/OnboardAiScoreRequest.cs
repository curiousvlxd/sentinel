using System.Text.Json.Serialization;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public sealed class OnboardAiScoreRequest
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "v1";

    [JsonPropertyName("bucket_start")]
    public string BucketStart { get; set; } = string.Empty;

    [JsonPropertyName("bucket_sec")]
    public int BucketSec { get; set; } = 60;

    [JsonPropertyName("features")]
    public Dictionary<string, SignalFeaturesDto> Features { get; set; } = new();
}

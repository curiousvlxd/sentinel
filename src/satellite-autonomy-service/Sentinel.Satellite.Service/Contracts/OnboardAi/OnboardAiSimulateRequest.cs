using System.Text.Json.Serialization;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public sealed class OnboardAiSimulateRequest
{
    [JsonPropertyName("scenario")]
    public string Scenario { get; set; } = "Normal";

    [JsonPropertyName("bucket_start")]
    public string? BucketStart { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }
}

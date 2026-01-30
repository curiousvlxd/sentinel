using System.Text.Json.Serialization;

namespace Sentinel.Core.Contracts;

public sealed class TelemetryHealthResponse
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; set; } = "v1";
    [JsonPropertyName("bucket_start")]
    public string BucketStart { get; set; } = string.Empty;
    public MlBlockContract Ml { get; set; } = new();
}

public sealed class MlBlockContract
{
    public ModelInfoContract Model { get; set; } = new();
    [JsonPropertyName("anomaly_score")]
    public double AnomalyScore { get; set; }
    public double Confidence { get; set; }
    [JsonPropertyName("per_signal_score")]
    public Dictionary<string, double> PerSignalScore { get; set; } = new();
    [JsonPropertyName("top_contributors")]
    public List<ContributorContract> TopContributors { get; set; } = new();
}

public sealed class ModelInfoContract
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public sealed class ContributorContract
{
    public string Key { get; set; } = string.Empty;
    public double Weight { get; set; }
}

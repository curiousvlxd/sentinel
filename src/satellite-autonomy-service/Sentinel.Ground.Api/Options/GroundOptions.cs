namespace Sentinel.Ground.Api.Options;

public sealed class GroundOptions
{
    public const string SectionName = "Ground";
    public string TimescaleConnectionString { get; set; } = string.Empty;
    public string GroundDbConnectionString { get; set; } = string.Empty;
    public string MlServiceBaseUrl { get; set; } = "http://localhost:8000";
    public double AnomalyThreshold { get; set; } = 0.7;
}

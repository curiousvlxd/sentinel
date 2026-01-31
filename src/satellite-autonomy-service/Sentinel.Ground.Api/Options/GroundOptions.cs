namespace Sentinel.Ground.Api.Options;

public sealed class GroundOptions
{
    public const string SectionName = "Ground";
    public double AnomalyThreshold { get; set; } = 0.7;
    /// <summary>Optional base URL of the first satellite service for sim proxy (e.g. http://localhost:5001).</summary>
    public string? SatelliteServiceBaseUrl { get; set; }
}

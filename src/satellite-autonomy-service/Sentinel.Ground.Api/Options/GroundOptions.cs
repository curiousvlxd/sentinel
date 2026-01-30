namespace Sentinel.Ground.Api.Options;

public sealed class GroundOptions
{
    public const string SectionName = "Ground";
    public double AnomalyThreshold { get; set; } = 0.7;
}

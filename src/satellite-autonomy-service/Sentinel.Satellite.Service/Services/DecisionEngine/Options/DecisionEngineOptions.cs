namespace Sentinel.Satellite.Service.Services.DecisionEngine.Options;

public sealed class DecisionEngineOptions
{
    public const string SectionName = "Ground";

    public double AnomalyThreshold { get; set; } = 0.7;
}

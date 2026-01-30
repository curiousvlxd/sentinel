namespace Sentinel.Satellite.Service.Options;

public sealed class SatelliteOptions
{
    public const string SectionName = "Satellite";

    public Guid? Id { get; set; }
    public string InstanceName { get; set; } = string.Empty;
    public string OnboardAiServiceName { get; set; } = "onboard-ai";
}

namespace Sentinel.Satellite.Service.Contracts.OnboardAi.Options;

public sealed class OnboardAiOptions
{
    public const string SectionName = "OnboardAi";

    public string BaseUrl { get; set; } = "http://localhost:8000";

    public string ServiceName { get; set; } = "onboard-ai";
}

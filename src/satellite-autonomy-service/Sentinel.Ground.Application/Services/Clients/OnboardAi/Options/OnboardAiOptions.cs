namespace Sentinel.Ground.Application.Services.Clients.OnboardAi.Options;

public sealed class OnboardAiOptions
{
    public const string SectionName = "OnboardAi";

    public string Url { get; init; } = "http://localhost:8000";
}

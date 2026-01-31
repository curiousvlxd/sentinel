namespace Sentinel.Ground.Application.Services.Clients.SatelliteService.Options;

public sealed class SatelliteServiceOptions
{
    public const string SectionName = "SatelliteService";

    public string Url { get; init; } = "http://localhost:5282";
}

namespace Sentinel.Ground.Api.Services.Seed.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string MissionId { get; set; } = string.Empty;

    public string SatelliteIds { get; set; } = string.Empty;
}

namespace Sentinel.Ground.Api.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string MissionId { get; set; } = "";
    public string SatelliteIds { get; set; } = "";
}

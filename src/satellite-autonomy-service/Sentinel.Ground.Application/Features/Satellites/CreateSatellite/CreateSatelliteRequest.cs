using Sentinel.Core.Enums;

namespace Sentinel.Ground.Application.Features.Satellites.CreateSatellite;

public sealed record CreateSatelliteRequest
{
    public string Name { get; set; } = string.Empty;

    public SatelliteMode Mode { get; set; } = SatelliteMode.Assisted;
}

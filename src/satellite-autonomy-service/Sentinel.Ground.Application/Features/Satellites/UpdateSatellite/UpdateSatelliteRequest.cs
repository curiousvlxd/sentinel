using Sentinel.Core.Enums;

namespace Sentinel.Ground.Application.Features.Satellites.UpdateSatellite;

public sealed record UpdateSatelliteRequest
{
    public string Name { get; set; } = string.Empty;

    public SatelliteStatus Status { get; set; }

    public SatelliteMode Mode { get; set; }

    public SatelliteState State { get; set; }

    public LinkStatus LinkStatus { get; set; }
}

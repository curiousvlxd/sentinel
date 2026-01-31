using Sentinel.Core.Enums;

namespace Sentinel.Ground.Application.Features.Satellites.Common.Contracts;

public sealed record SatelliteResponse
{
    public Guid Id { get; set; }

    public Guid? MissionId { get; set; }

    public string? MissionName { get; set; }

    public string Name { get; set; } = string.Empty;

    public SatelliteStatus Status { get; set; }

    public SatelliteMode Mode { get; set; }

    public SatelliteState State { get; set; }

    public LinkStatus LinkStatus { get; set; }

    public string? LastBucketStart { get; set; }

    public string CreatedAt { get; set; } = string.Empty;
}

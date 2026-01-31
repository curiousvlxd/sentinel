using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class Satellite
{
    public Guid Id { get; set; }
    public Guid? MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? NoradId { get; set; }
    public string? ExternalId { get; set; }
    public SatelliteStatus Status { get; set; }
    public SatelliteMode Mode { get; set; }
    public SatelliteState State { get; set; }
    public LinkStatus LinkStatus { get; set; }
    public DateTime? LastBucketStart { get; set; }
    public DateTime CreatedAt { get; set; }
}

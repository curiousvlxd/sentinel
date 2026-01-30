using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class Decision
{
    public Guid Id { get; set; }
    public Guid SatelliteId { get; set; }
    public DateTime BucketStart { get; set; }
    public DecisionType DecisionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Metadata { get; set; }
}

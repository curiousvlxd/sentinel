using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class Decision
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public Satellite? Satellite { get; set; }

    public DateTimeOffset BucketStart { get; set; }

    public DecisionType DecisionType { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string? Metadata { get; set; }
}

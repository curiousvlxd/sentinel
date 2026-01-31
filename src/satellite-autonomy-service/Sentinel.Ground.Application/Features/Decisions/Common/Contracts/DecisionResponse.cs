using Sentinel.Core.Enums;

namespace Sentinel.Ground.Application.Features.Decisions.Common.Contracts;

public sealed record DecisionResponse
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public string BucketStart { get; set; } = string.Empty;

    public DecisionType Type { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;
}

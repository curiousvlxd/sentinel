using Sentinel.Core.Enums;

namespace Sentinel.Ground.Application.Features.Operations.Common.Contracts;

public sealed record SatelliteOperationResponse
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public Guid? MissionId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? PayloadJson { get; set; }

    public int Priority { get; set; }

    public int TtlSec { get; set; }

    public SatelliteOperationStatus Status { get; set; }

    public string CreatedAt { get; set; } = string.Empty;

    public string? ClaimedAt { get; set; }

    public string? ExecutedAt { get; set; }
}

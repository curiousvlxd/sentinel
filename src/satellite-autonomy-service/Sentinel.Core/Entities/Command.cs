using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class Command
{
    public Guid Id { get; set; }
    public Guid SatelliteId { get; set; }
    public Guid? MissionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public int Priority { get; set; }
    public int TtlSec { get; set; }
    public CommandStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
}

namespace Sentinel.Core.Contracts;

public sealed class CommandDto
{
    public Guid Id { get; set; }
    public Guid SatelliteId { get; set; }
    public Guid? MissionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public int Priority { get; set; }
    public int TtlSec { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? ClaimedAt { get; set; }
    public string? ExecutedAt { get; set; }
}

public sealed class CommandCreateRequest
{
    public string Type { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int TtlSec { get; set; }
    public string? PayloadJson { get; set; }
}

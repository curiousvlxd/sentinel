using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class SatelliteOperation
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public Satellite? Satellite { get; set; }

    public Guid? MissionId { get; set; }

    public Mission? Mission { get; set; }

    public Guid? CommandTemplateId { get; set; }

    public CommandTemplate? CommandTemplate { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? PayloadJson { get; set; }

    public int Priority { get; set; }

    public int TtlSec { get; set; }

    public SatelliteOperationStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ClaimedAt { get; set; }

    public DateTimeOffset? ExecutedAt { get; set; }

    public static SatelliteOperation Create(
        Guid satelliteId,
        Guid? missionId,
        Guid? commandTemplateId,
        string type,
        string? payloadJson,
        int priority,
        int ttlSec)
    {
        return new SatelliteOperation
        {
            Id = Guid.NewGuid(),
            SatelliteId = satelliteId,
            MissionId = missionId,
            CommandTemplateId = commandTemplateId,
            Type = type,
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? null : payloadJson.Trim(),
            Priority = priority,
            TtlSec = ttlSec,
            Status = SatelliteOperationStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

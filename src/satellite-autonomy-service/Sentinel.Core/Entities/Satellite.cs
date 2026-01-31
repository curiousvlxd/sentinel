using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class Satellite
{
    public Guid Id { get; set; }

    public Guid? MissionId { get; set; }

    public Mission? Mission { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? NoradId { get; set; }

    public string? ExternalId { get; set; }

    public SatelliteStatus Status { get; set; }

    public SatelliteMode Mode { get; set; }

    public SatelliteState State { get; set; }

    public LinkStatus LinkStatus { get; set; }

    public DateTimeOffset? LastBucketStart { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static Satellite Create(string name, Guid? missionId, SatelliteMode mode)
    {
        return new Satellite
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            MissionId = missionId,
            Status = SatelliteStatus.Active,
            Mode = mode,
            State = SatelliteState.Ok,
            LinkStatus = LinkStatus.Offline,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, SatelliteStatus status, SatelliteMode mode, SatelliteState state, LinkStatus linkStatus)
    {
        Name = name.Trim();
        Status = status;
        Mode = mode;
        State = state;
        LinkStatus = linkStatus;
    }
}

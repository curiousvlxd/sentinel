namespace Sentinel.Ground.Application.Features.Missions.UpdateMission;

public sealed record UpdateMissionRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

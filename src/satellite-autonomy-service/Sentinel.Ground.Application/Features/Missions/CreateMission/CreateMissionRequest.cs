namespace Sentinel.Ground.Application.Features.Missions.CreateMission;

public sealed record CreateMissionRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

namespace Sentinel.Ground.Application.Features.Missions.Common.Contracts;

public sealed record MissionResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public string CreatedAt { get; set; } = string.Empty;
}

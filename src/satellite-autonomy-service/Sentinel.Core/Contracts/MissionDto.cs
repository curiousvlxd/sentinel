namespace Sentinel.Core.Contracts;

public sealed class MissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public sealed class MissionCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class MissionUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

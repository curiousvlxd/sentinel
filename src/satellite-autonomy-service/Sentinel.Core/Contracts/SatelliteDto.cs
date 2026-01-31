namespace Sentinel.Core.Contracts;

public sealed class SatelliteDto
{
    public Guid Id { get; set; }
    public Guid? MissionId { get; set; }
    public string? MissionName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string LinkStatus { get; set; } = string.Empty;
    public string? LastBucketStart { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public sealed class SatelliteCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Mode { get; set; } = "Assisted";
}

public sealed class SatelliteUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string LinkStatus { get; set; } = string.Empty;
}

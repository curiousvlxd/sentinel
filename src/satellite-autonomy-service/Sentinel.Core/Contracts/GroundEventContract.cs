namespace Sentinel.Core.Contracts;

public sealed class GroundEventContract
{
    public Guid EventId { get; set; }
    public Guid? MissionId { get; set; }
    public Guid SatelliteId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Ts { get; set; } = string.Empty;
    public string BucketStart { get; set; } = string.Empty;
    public object? Payload { get; set; }
}

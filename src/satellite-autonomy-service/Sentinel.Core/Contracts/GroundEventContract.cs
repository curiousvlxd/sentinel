namespace Sentinel.Core.Contracts;

public sealed class GroundEventContract
{
    public string Type { get; set; } = string.Empty;
    public Guid SatelliteId { get; set; }
    public string BucketStart { get; set; } = string.Empty;
    public object? Payload { get; set; }
}

namespace Sentinel.Core.Contracts;

public sealed class DecisionResponse
{
    public Guid Id { get; set; }
    public Guid SatelliteId { get; set; }
    public string BucketStart { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

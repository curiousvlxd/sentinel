namespace Sentinel.Core.Contracts;

public sealed class TelemetryIngestResponse
{
    public bool Accepted { get; set; }
    public string StoredAt { get; set; } = string.Empty;
    public Guid SatelliteId { get; set; }
}

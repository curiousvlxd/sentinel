namespace Sentinel.Core.Contracts.Telemetry;

public sealed record TelemetryIngestResponse
{
    public bool Accepted { get; set; }

    public DateTimeOffset StoredAt { get; set; }

    public Guid SatelliteId { get; set; }
}

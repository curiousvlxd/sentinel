using Sentinel.Core.Contracts.Telemetry;

namespace Sentinel.Core.Contracts.Events;

public sealed record GroundEventContract
{
    public Guid EventId { get; set; }

    public Guid? MissionId { get; set; }

    public Guid SatelliteId { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTimeOffset Ts { get; set; }

    public DateTimeOffset? BucketStart { get; set; }

    public object? Payload { get; set; }

    public static GroundEventContract CreateTelemetry(
        Guid missionId,
        Guid satelliteId,
        DateTimeOffset ts,
        LocationContract location,
        TelemetrySignalsContract signals)
    {
        var payload = new { timestamp = ts, location, signals };
        return new GroundEventContract
        {
            MissionId = missionId,
            SatelliteId = satelliteId,
            Type = "telemetry",
            Ts = ts,
            Payload = payload
        };
    }
}

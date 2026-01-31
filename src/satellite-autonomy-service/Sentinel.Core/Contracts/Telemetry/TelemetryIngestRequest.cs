namespace Sentinel.Core.Contracts.Telemetry;

public sealed record TelemetryIngestRequest
{
    public string SchemaVersion { get; set; } = "v1";

    public Guid MissionId { get; set; }

    public Guid SatelliteId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public LocationContract? Location { get; set; }

    public TelemetrySignalsContract Signals { get; set; } = new();

    public string? Source { get; set; }

    public static TelemetryIngestRequest Create(
        Guid missionId,
        Guid satelliteId,
        DateTimeOffset timestamp,
        LocationContract location,
        TelemetrySignalsContract signals,
        string source = "sim")
    {
        return new TelemetryIngestRequest
        {
            SchemaVersion = "v1",
            MissionId = missionId,
            SatelliteId = satelliteId,
            Timestamp = timestamp,
            Source = source,
            Location = location,
            Signals = signals
        };
    }
}

public sealed record LocationContract
{
    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public double? AltKm { get; set; }

    public static LocationContract Create(double? lat, double? lon, double? altKm)
    {
        return new LocationContract { Lat = lat, Lon = lon, AltKm = altKm };
    }
}

public sealed record TelemetrySignalsContract
{
    public double CpuTemperature { get; set; }

    public double BatteryVoltage { get; set; }

    public double Pressure { get; set; }

    public double GyroSpeed { get; set; }

    public double SignalStrength { get; set; }

    public double PowerConsumption { get; set; }
}

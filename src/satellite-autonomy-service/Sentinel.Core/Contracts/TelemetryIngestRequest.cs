namespace Sentinel.Core.Contracts;

public sealed class TelemetryIngestRequest
{
    public string SchemaVersion { get; set; } = "v1";
    public Guid MissionId { get; set; }
    public Guid SatelliteId { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public LocationContract? Location { get; set; }
    public TelemetrySignalsContract Signals { get; set; } = new();
    public string? Source { get; set; }
}

public sealed class LocationContract
{
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public double? AltKm { get; set; }
}

public sealed class TelemetrySignalsContract
{
    public double CpuTemperature { get; set; }
    public double BatteryVoltage { get; set; }
    public double Pressure { get; set; }
    public double GyroSpeed { get; set; }
    public double SignalStrength { get; set; }
    public double PowerConsumption { get; set; }
}

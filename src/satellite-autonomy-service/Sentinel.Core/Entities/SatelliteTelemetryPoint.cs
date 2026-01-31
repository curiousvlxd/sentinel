namespace Sentinel.Core.Entities;

public sealed class SatelliteTelemetryPoint
{
    public Guid SatelliteId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public double CpuTemperature { get; set; }

    public double BatteryVoltage { get; set; }

    public double Pressure { get; set; }

    public double GyroSpeed { get; set; }

    public double SignalStrength { get; set; }

    public double PowerConsumption { get; set; }

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public double? AltKm { get; set; }

    public string? Source { get; set; }
}

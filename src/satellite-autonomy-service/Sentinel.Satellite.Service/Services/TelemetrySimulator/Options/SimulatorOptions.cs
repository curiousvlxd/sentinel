namespace Sentinel.Satellite.Service.Services.TelemetrySimulator.Options;

public sealed class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public required Guid MissionId { get; set; }

    public required Guid SatelliteId { get; set; }
}

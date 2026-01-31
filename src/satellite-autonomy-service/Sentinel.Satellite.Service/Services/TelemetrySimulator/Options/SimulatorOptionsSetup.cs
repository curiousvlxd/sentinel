using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Services.TelemetrySimulator.Options;

public sealed class SimulatorOptionsSetup(IConfiguration configuration) : IConfigureOptions<SimulatorOptions>
{
    public void Configure(SimulatorOptions options)
    {
        var section = configuration.GetSection(SimulatorOptions.SectionName);
        var missionIdStr = section["MissionId"];
        var satelliteIdStr = section["SatelliteId"] ?? configuration["Satellite:Id"];

        if (string.IsNullOrEmpty(missionIdStr) || !Guid.TryParse(missionIdStr, out var missionId))
            throw new InvalidOperationException("Simulator:MissionId is required and must be a valid GUID.");
        if (string.IsNullOrEmpty(satelliteIdStr) || !Guid.TryParse(satelliteIdStr, out var satelliteId))
            throw new InvalidOperationException("Simulator:SatelliteId (or Satellite:Id) is required and must be a valid GUID.");

        options.MissionId = missionId;
        options.SatelliteId = satelliteId;
    }
}

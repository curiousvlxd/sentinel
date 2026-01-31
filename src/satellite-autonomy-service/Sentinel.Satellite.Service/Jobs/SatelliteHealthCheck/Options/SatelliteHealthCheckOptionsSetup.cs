using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Jobs.SatelliteHealthCheck.Options;

public sealed class SatelliteHealthCheckOptionsSetup(IConfiguration configuration) : IConfigureOptions<SatelliteHealthCheckOptions>
{
    public void Configure(SatelliteHealthCheckOptions options) =>
        configuration.GetSection(SatelliteHealthCheckOptions.SectionName).Bind(options);
}

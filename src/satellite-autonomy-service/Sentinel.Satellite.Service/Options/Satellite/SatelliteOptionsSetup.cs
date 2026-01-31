using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Options.Satellite;

public sealed class SatelliteOptionsSetup(IConfiguration configuration) : IConfigureOptions<SatelliteOptions>
{
    public void Configure(SatelliteOptions options) => configuration.GetSection(SatelliteOptions.SectionName).Bind(options);
}

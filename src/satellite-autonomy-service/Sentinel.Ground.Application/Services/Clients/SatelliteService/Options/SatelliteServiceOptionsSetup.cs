using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sentinel.Ground.Application.Services.Clients.SatelliteService.Options;

public sealed class SatelliteServiceOptionsSetup(IConfiguration configuration) : IConfigureOptions<SatelliteServiceOptions>
{
    public void Configure(SatelliteServiceOptions options) => configuration.GetSection(SatelliteServiceOptions.SectionName).Bind(options);
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sentinel.AppHost.Configuration;

public sealed class SentinelHostOptionsSetup(IConfiguration configuration) : IConfigureOptions<SentinelHostOptions>
{
    public void Configure(SentinelHostOptions options) => configuration.GetSection(SentinelHostOptions.SectionName).Bind(options);
}

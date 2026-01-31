using Microsoft.Extensions.Options;

namespace Sentinel.Ground.Api.Services.Seed.Options;

public sealed class SeedOptionsSetup(IConfiguration configuration) : IConfigureOptions<SeedOptions>
{
    public void Configure(SeedOptions options) => configuration.GetSection(SeedOptions.SectionName).Bind(options);
}

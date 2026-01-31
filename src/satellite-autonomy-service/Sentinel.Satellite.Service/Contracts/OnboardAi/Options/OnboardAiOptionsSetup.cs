using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi.Options;

public sealed class OnboardAiOptionsSetup(IConfiguration configuration) : IConfigureOptions<OnboardAiOptions>
{
    public void Configure(OnboardAiOptions options) =>
        configuration.GetSection(OnboardAiOptions.SectionName).Bind(options);
}

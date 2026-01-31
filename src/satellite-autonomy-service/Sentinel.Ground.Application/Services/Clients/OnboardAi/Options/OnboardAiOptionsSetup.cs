using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sentinel.Ground.Application.Services.Clients.OnboardAi.Options;

public sealed class OnboardAiOptionsSetup(IConfiguration configuration) : IConfigureOptions<OnboardAiOptions>
{
    public void Configure(OnboardAiOptions options) => configuration.GetSection(OnboardAiOptions.SectionName).Bind(options);
}

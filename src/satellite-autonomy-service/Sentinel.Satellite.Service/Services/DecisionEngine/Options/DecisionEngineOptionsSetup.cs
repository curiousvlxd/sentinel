using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Services.DecisionEngine.Options;

public sealed class DecisionEngineOptionsSetup(IConfiguration configuration) : IConfigureOptions<DecisionEngineOptions>
{
    public void Configure(DecisionEngineOptions options) => configuration.GetSection(DecisionEngineOptions.SectionName).Bind(options);
}

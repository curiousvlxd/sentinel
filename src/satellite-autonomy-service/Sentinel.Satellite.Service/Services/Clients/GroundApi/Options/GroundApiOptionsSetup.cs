using Microsoft.Extensions.Options;

namespace Sentinel.Satellite.Service.Services.Clients.GroundApi.Options;

public sealed class GroundApiOptionsSetup(IConfiguration configuration) : IConfigureOptions<GroundApiOptions>
{
    public void Configure(GroundApiOptions options)
    {
        configuration.GetSection(GroundApiOptions.SectionName).Bind(options);
        var url = configuration["Services:ground-api:http:0"]?.Trim();
        if (!string.IsNullOrEmpty(url)) options.BaseUrl = url;

        options.BaseUrl = UrlNormalizer.NormalizeHttpUrl(options.BaseUrl);
    }
}

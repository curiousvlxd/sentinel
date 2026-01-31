using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Refit;
using Sentinel.Ground.Application.Services.Clients.OnboardAi;
using Sentinel.Ground.Application.Services.Clients.OnboardAi.Options;
using Sentinel.Ground.Application.Services.Clients.SatelliteService;
using Sentinel.Ground.Application.Services.Clients.SatelliteService.Options;

namespace Sentinel.Ground.Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ApplicationAssemblyReference.Assembly);
        });
        builder.Services.ConfigureOptions<OnboardAiOptionsSetup>();
        builder.Services.ConfigureOptions<SatelliteServiceOptionsSetup>();
        builder.Services.AddRefitClient<IOnboardAiClient>(sp => new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions)
        }).ConfigureHttpClient((sp, c) =>
        {
            var opts = sp.GetRequiredService<IOptions<OnboardAiOptions>>().Value;
            c.BaseAddress = new Uri(opts.Url);
        });
        builder.Services.AddRefitClient<ISatelliteServiceClient>(sp => new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions)
        }).ConfigureHttpClient((sp, c) =>
        {
            var opts = sp.GetRequiredService<IOptions<SatelliteServiceOptions>>().Value;
            c.BaseAddress = new Uri(opts.Url);
        });
        return builder;
    }
}

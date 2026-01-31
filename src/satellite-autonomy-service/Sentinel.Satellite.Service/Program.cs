using Refit;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Sentinel.Satellite.Service.Constants;
using Sentinel.Satellite.Service.Contracts.OnboardAi.Options;
using Sentinel.Satellite.Service.Jobs.SatelliteHealthCheck;
using Sentinel.Satellite.Service.Jobs.SatelliteHealthCheck.Options;
using Sentinel.Satellite.Service.Options.Satellite;
using Sentinel.Satellite.Service.Services;
using Sentinel.Satellite.Service.Services.Clients.GroundApi;
using Sentinel.Satellite.Service.Services.Clients.GroundApi.Options;
using Sentinel.Satellite.Service.Services.DecisionEngine;
using Sentinel.Satellite.Service.Services.DecisionEngine.Options;
using Sentinel.Satellite.Service.Services.TelemetrySimulator;
using Sentinel.Satellite.Service.Services.TelemetrySimulator.Options;
using Sentinel.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddKeyedNpgsqlDataSource(DataSources.Satellite);
builder.AddKeyedNpgsqlDataSource(DataSources.Ground);

builder.Services.ConfigureOptions<SatelliteOptionsSetup>();
builder.Services.ConfigureOptions<SatelliteHealthCheckOptionsSetup>();
builder.Services.ConfigureOptions<DecisionEngineOptionsSetup>();
builder.Services.ConfigureOptions<GroundApiOptionsSetup>();
builder.Services.ConfigureOptions<SimulatorOptionsSetup>();
builder.Services.ConfigureOptions<OnboardAiOptionsSetup>();

builder.Services.AddRefitClient<IGroundApiClient>(_ => new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
}).ConfigureHttpClient((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GroundApiOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddRefitClient<Sentinel.Satellite.Service.Contracts.OnboardAi.IOnboardAiClient>(_ => new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
}).ConfigureHttpClient((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<OnboardAiOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddSingleton<TelemetryBucketReader>();
builder.Services.AddSingleton<DecisionEngine>();
builder.Services.AddSingleton<TelemetryIngestService>();
builder.Services.AddSingleton<TelemetrySimulatorService>();
builder.Services.AddHostedService<SatelliteHealthCheckJob>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelemetrySimulatorService>());

var app = builder.Build();

app.UseExceptionHandlerDefaults();
app.UseCors();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

app.Run();

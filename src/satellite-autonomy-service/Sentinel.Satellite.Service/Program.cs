using Refit;
using Sentinel.Satellite.Service.Contracts.OnboardAi;
using Sentinel.Satellite.Service.Jobs;
using Sentinel.Satellite.Service.Options;
using Sentinel.Satellite.Service.Services;
using Sentinel.ServiceDefaults.Extensions;
using System.Text.Json;

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

builder.AddKeyedNpgsqlDataSource("satellite");
builder.AddKeyedNpgsqlDataSource("ground");

builder.Services.Configure<GroundOptions>(builder.Configuration.GetSection(GroundOptions.SectionName));
builder.Services.Configure<SatelliteHealthCheckOptions>(builder.Configuration.GetSection(SatelliteHealthCheckOptions.SectionName));
builder.Services.Configure<SatelliteOptions>(builder.Configuration.GetSection(SatelliteOptions.SectionName));

builder.Services.AddSingleton<TelemetryBucketReader>(sp => new TelemetryBucketReader(sp.GetRequiredKeyedService<Npgsql.NpgsqlDataSource>("satellite")));
var onboardAiServiceName = builder.Configuration["Satellite:OnboardAiServiceName"] ?? "onboard-ai";
var refitJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
builder.Services.AddRefitClient<IOnboardAiClient>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(refitJson) })
    .ConfigureHttpClient(c => c.BaseAddress = new Uri($"https+http://{onboardAiServiceName}"));
builder.Services.AddHttpClient<GroundEventsClient>(c => c.BaseAddress = new Uri("https+http://ground-api"));
builder.Services.AddSingleton<DecisionEngine>();

builder.Services.AddSingleton<TelemetryIngestService>();
builder.Services.AddSingleton<TelemetrySimulatorService>();
builder.Services.AddHostedService<SatelliteHealthCheckJob>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelemetrySimulatorService>());

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

app.Run();

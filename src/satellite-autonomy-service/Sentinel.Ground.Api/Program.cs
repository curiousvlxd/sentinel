using Hangfire;
using Hangfire.PostgreSql;
using Sentinel.Ground.Api.Jobs;
using Sentinel.Ground.Api.Services;
using Sentinel.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var groundSection = builder.Configuration.GetSection("Ground");
var timescaleCs = groundSection["TimescaleConnectionString"] ?? "";
var groundDbCs = groundSection["GroundDbConnectionString"] ?? "";
var mlBaseUrl = groundSection["MlServiceBaseUrl"] ?? "http://localhost:8000";

builder.Services.AddSingleton<TelemetryBucketReader>();
builder.Services.AddSingleton<DecisionEngine>();
builder.Services.AddSingleton<MlResultRepository>();
builder.Services.AddSingleton<DecisionRepository>();
builder.Services.AddSingleton<SatelliteRepository>();
builder.Services.AddSingleton<SseEventBus>();
builder.Services.AddSingleton<SatelliteHealthCheckJob>();
builder.Services.AddSingleton<SeedService>();

builder.Services.AddHttpClient<MlClient>(client => { client.BaseAddress = new Uri(mlBaseUrl); });

if (!string.IsNullOrEmpty(groundDbCs))
{
    builder.Services.AddHangfire(c => c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(groundDbCs));
    builder.Services.AddHangfireServer();
}

var app = builder.Build();

app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

if (!string.IsNullOrEmpty(groundDbCs))
{
    app.MapHangfireDashboard("/hangfire");
    RecurringJob.AddOrUpdate<SatelliteHealthCheckJob>("satellite-health", j => j.RunAsync(null, default), "*/1 * * * *");
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<SeedService>().SeedIfEmptyAsync();
}

app.Run();

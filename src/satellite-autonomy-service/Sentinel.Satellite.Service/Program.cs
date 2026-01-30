using Sentinel.Satellite.Service.Services;
using Sentinel.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<TelemetryIngestService>();
builder.Services.AddSingleton<TelemetrySimulatorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelemetrySimulatorService>());

var app = builder.Build();

app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

app.Run();

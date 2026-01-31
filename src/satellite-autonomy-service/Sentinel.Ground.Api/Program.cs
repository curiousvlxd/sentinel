using System.Text.Json.Serialization;
using Sentinel.Ground.Api.Services;
using Sentinel.Ground.Api.Services.Seed.Options;
using Sentinel.Ground.Application;
using Sentinel.Infrastructure.Extensions;
using Sentinel.Infrastructure.Persistence;
using Sentinel.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplication();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        ServiceDefaultsExtensions.ConfigureJsonOptions(o);
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddPersistence();
builder.Services.ConfigureOptions<SeedOptionsSetup>();
builder.Services.AddSingleton<SseEventBus>();
builder.Services.AddScoped<SeedService>();

var app = builder.Build();

app.UseExceptionHandlerDefaults();
app.UseCors();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

await app.MigrateGroundDbIfNeededAsync();

using (var scope = app.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<SeedService>().SeedIfEmptyAsync();

app.Run();

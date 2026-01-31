using Sentinel.Ground.Api.Options;
using Sentinel.Ground.Api.Services;
using Sentinel.Infrastructure.Extensions;
using Sentinel.Infrastructure.Persistence;
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
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddPersistence();
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.SectionName));
builder.Services.AddSingleton<SseEventBus>();
builder.Services.AddScoped<SeedService>();

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapOpenApi();

await app.MigrateGroundDbIfNeededAsync();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<SeedService>().SeedIfEmptyAsync();

app.Run();

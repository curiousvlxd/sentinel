using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Infrastructure.Persistence.Options;

namespace Sentinel.Infrastructure.Extensions;

public static class MigrationExtension
{
    private const int MaxRetries = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public static async Task MigrateGroundDbIfNeededAsync(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Sentinel.Infrastructure.Migrations");
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            using (var scope = host.Services.CreateScope())
            {
                var options = scope.ServiceProvider.GetService<IOptions<GroundDatabaseOptions>>()?.Value;
                if (options?.MigrateOnStartup != true)
                    return;
                var db = scope.ServiceProvider.GetRequiredService<IGroundDbContext>();
                try
                {
                    await db.Database.MigrateAsync();
                    return;
                }
                catch (NpgsqlException ex) when (attempt < MaxRetries)
                {
                    logger.LogWarning(ex, "Ground DB migration attempt {Attempt}/{Max} failed, retrying in {Delay}s.",
                        attempt, MaxRetries, RetryDelay.TotalSeconds);
                }
            }
            if (attempt < MaxRetries)
                await Task.Delay(RetryDelay);
        }
    }
}

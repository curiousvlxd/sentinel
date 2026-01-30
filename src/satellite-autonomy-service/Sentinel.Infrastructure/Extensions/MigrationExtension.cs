using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Infrastructure.Persistence.Options;

namespace Sentinel.Infrastructure.Extensions;

public static class MigrationExtension
{
    public static async Task MigrateGroundDbIfNeededAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var options = scope.ServiceProvider.GetService<IOptions<GroundDatabaseOptions>>()?.Value;
        if (options?.MigrateOnStartup != true)
            return;
        var db = scope.ServiceProvider.GetRequiredService<IGroundDbContext>();
        await db.Database.MigrateAsync();
    }
}

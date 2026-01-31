using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Infrastructure.Persistence.Options;

namespace Sentinel.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.ConfigureOptions<GroundDatabaseOptionsSetup>();
        services.AddDbContext<GroundDbContext>((sp, o) =>
        {
            var options = sp.GetRequiredService<IOptions<GroundDatabaseOptions>>().Value;
            var env = sp.GetRequiredService<IHostEnvironment>();
            var connectionString = EnsureNoSslForLocalDev(options.ConnectionString, env.IsDevelopment());
            o.UseNpgsqlWithSnakeCase(connectionString);
            o.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
        services.AddScoped<IGroundDbContext>(sp => sp.GetRequiredService<GroundDbContext>());
        return services;
    }

    private static string EnsureNoSslForLocalDev(string connectionString, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;

        var csb = new NpgsqlConnectionStringBuilder(connectionString);
        var host = csb.Host?.Trim() ?? string.Empty;
        var isLocalHost = host.Length == 0 ||
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host == "127.0.0.1" ||
            host.Equals("ground-db", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase);
        if (!isDevelopment && !isLocalHost) return connectionString;

        csb.SslMode = SslMode.Disable;
        return csb.ConnectionString;
    }
}

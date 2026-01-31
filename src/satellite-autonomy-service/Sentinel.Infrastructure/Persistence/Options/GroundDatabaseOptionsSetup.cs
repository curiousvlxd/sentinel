using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Sentinel.Core.Configuration;

namespace Sentinel.Infrastructure.Persistence.Options;

public sealed class GroundDatabaseOptionsSetup(IConfiguration configuration)
    : IConfigureOptions<GroundDatabaseOptions>
{
    public void Configure(GroundDatabaseOptions options)
    {
        configuration.GetSection(GroundDatabaseOptions.SectionName).Bind(options);
        var connSection = configuration.GetSection(ConnectionStringsKeys.SectionName);
        var cs = connSection["ground"] ?? connSection["Ground"] ?? connSection[ConnectionStringsKeys.GroundDb];
        if (string.IsNullOrWhiteSpace(cs))
            return;
        var csb = new NpgsqlConnectionStringBuilder(cs);
        if (ShouldDisableSsl(csb))
            csb.SslMode = SslMode.Disable;
        options.ConnectionString = csb.ConnectionString;
    }

    private bool ShouldDisableSsl(NpgsqlConnectionStringBuilder csb)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? configuration["DOTNET_ENVIRONMENT"] ?? "";
        if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            return true;
        if (csb.SslMode is SslMode.Require or SslMode.VerifyFull or SslMode.VerifyCA)
            return false;
        var host = csb.Host?.Trim() ?? "";
        return host.Length == 0 ||
               host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host == "127.0.0.1" ||
               host.Equals("ground-db", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase);
    }
}

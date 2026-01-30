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
        var section = configuration.GetSection(ConnectionStringsKeys.SectionName);
        var cs = section["ground"] ?? section[ConnectionStringsKeys.GroundDb];
        if (string.IsNullOrEmpty(cs))
            return;
        var builder = new NpgsqlConnectionStringBuilder(cs);
        if (IsLocalServer(builder.Host))
            builder.SslMode = SslMode.Disable;
        options.ConnectionString = builder.ConnectionString;
    }

    private static bool IsLocalServer(string? host)
    {
        if (string.IsNullOrEmpty(host))
            return true;
        return host == "localhost" || host == "127.0.0.1" || host == "ground-db";
    }
}

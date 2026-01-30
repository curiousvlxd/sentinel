using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Sentinel.Infrastructure.Persistence;

public static class NpgsqlSnakeCaseExtensions
{
    public static DbContextOptionsBuilder UseNpgsqlWithSnakeCase(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptions = null)
    {
        var builder = optionsBuilder
            .UseNpgsql(connectionString, npgsqlOptions)
            .UseSnakeCaseNamingConvention();
        return builder;
    }
}

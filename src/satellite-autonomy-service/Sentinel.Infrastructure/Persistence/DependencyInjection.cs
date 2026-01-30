using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            o.UseNpgsqlWithSnakeCase(options.ConnectionString);
        });
        services.AddScoped<IGroundDbContext>(sp => sp.GetRequiredService<GroundDbContext>());
        return services;
    }
}

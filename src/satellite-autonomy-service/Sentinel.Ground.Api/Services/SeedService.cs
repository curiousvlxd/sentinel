using Npgsql;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class SeedService
{
    private readonly string _connectionString;
    private readonly ILogger<SeedService> _logger;

    public SeedService(IConfiguration configuration, ILogger<SeedService> logger)
    {
        var section = configuration.GetSection(GroundOptions.SectionName);
        _connectionString = section["GroundDbConnectionString"] ?? string.Empty;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using (var cmd = new NpgsqlCommand("SELECT 1 FROM missions LIMIT 1", conn))
        using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken)) return;
        }

        var missionId = Guid.NewGuid();
        var satelliteId = Guid.NewGuid();
        await using (var cmd = new NpgsqlCommand(
            "INSERT INTO missions (id, name, description, created_at, is_active) VALUES (@id, @name, @desc, @created, true)",
            conn))
        {
            cmd.Parameters.AddWithValue("id", missionId);
            cmd.Parameters.AddWithValue("name", "Demo Mission");
            cmd.Parameters.AddWithValue("desc", "Hackathon demo");
            cmd.Parameters.AddWithValue("created", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var cmd = new NpgsqlCommand(
            "INSERT INTO satellites (id, mission_id, name, status, created_at) VALUES (@id, @mid, @name, 'Active', @created)",
            conn))
        {
            cmd.Parameters.AddWithValue("id", satelliteId);
            cmd.Parameters.AddWithValue("mid", missionId);
            cmd.Parameters.AddWithValue("name", "Demo Satellite");
            cmd.Parameters.AddWithValue("created", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        _logger.LogInformation("Seeded mission {MissionId} and satellite {SatelliteId}. Set Simulator:MissionId and Simulator:SatelliteId in Satellite.Service.", missionId, satelliteId);
    }
}

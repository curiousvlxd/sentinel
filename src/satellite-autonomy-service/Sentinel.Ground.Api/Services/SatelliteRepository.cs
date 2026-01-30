using Npgsql;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class SatelliteRepository
{
    private readonly string _connectionString;

    public SatelliteRepository(IConfiguration configuration)
    {
        var section = configuration.GetSection(GroundOptions.SectionName);
        _connectionString = section["GroundDbConnectionString"] ?? string.Empty;
    }

    public async Task<IReadOnlyList<Satellite>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString))
            return Array.Empty<Satellite>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand("SELECT id, mission_id, name, norad_id, external_id, status, created_at FROM satellites WHERE status = 'Active'", conn);
        var list = new List<Satellite>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(new Satellite
            {
                Id = reader.GetGuid(0),
                MissionId = reader.GetGuid(1),
                Name = reader.GetString(2),
                NoradId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                ExternalId = reader.IsDBNull(4) ? null : reader.GetString(4),
                Status = Enum.TryParse<SatelliteStatus>(reader.GetString(5), out var s) ? s : SatelliteStatus.Active,
                CreatedAt = reader.GetDateTime(6)
            });
        return list;
    }
}

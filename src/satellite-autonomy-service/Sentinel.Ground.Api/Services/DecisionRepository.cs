using Npgsql;
using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class DecisionRepository
{
    private readonly string _connectionString;

    public DecisionRepository(IConfiguration configuration)
    {
        var section = configuration.GetSection(GroundOptions.SectionName);
        _connectionString = section["GroundDbConnectionString"] ?? string.Empty;
    }

    public async Task<Guid> InsertAsync(Decision entity, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        var id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO decisions (id, satellite_id, bucket_start, decision_type, reason, created_at, metadata)
            VALUES (@id, @sid, @bucket, @type::text, @reason, @created, @meta::jsonb)
            RETURNING id
            """,
            conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("sid", entity.SatelliteId);
        cmd.Parameters.AddWithValue("bucket", entity.BucketStart);
        cmd.Parameters.AddWithValue("type", entity.DecisionType.ToString());
        cmd.Parameters.AddWithValue("reason", entity.Reason);
        cmd.Parameters.AddWithValue("created", entity.CreatedAt);
        cmd.Parameters.AddWithValue("meta", (object?)entity.Metadata ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetGuid(0);
    }

    public async Task<IReadOnlyList<DecisionResponse>> GetBySatelliteAsync(Guid satelliteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString))
            return Array.Empty<DecisionResponse>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, satellite_id, bucket_start, decision_type, reason, created_at FROM decisions WHERE satellite_id = @sid AND bucket_start >= @from AND bucket_start <= @to ORDER BY bucket_start DESC",
            conn);
        cmd.Parameters.AddWithValue("sid", satelliteId);
        cmd.Parameters.AddWithValue("from", from);
        cmd.Parameters.AddWithValue("to", to);
        var list = new List<DecisionResponse>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(new DecisionResponse
            {
                Id = reader.GetGuid(0),
                SatelliteId = reader.GetGuid(1),
                BucketStart = reader.GetDateTime(2).ToUniversalTime().ToString("O"),
                Type = reader.GetString(3),
                Reason = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5).ToUniversalTime().ToString("O")
            });
        return list;
    }
}

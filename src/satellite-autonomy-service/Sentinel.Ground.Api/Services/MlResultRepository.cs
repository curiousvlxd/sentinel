using Npgsql;
using System.Text.Json;
using Sentinel.Core.Entities;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class MlResultRepository
{
    private readonly string _connectionString;

    public MlResultRepository(IConfiguration configuration)
    {
        var section = configuration.GetSection(GroundOptions.SectionName);
        _connectionString = section["GroundDbConnectionString"] ?? string.Empty;
    }

    public async Task<Guid> InsertAsync(MlHealthResult entity, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO ml_health_results (id, satellite_id, bucket_start, model_name, model_version, anomaly_score, confidence, per_signal_score, top_contributors, created_at)
            VALUES (@id, @sid, @bucket, @model_name, @model_ver, @score, @conf, @per_signal::jsonb, @top::jsonb, @created)
            RETURNING id
            """,
            conn);
        var id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("sid", entity.SatelliteId);
        cmd.Parameters.AddWithValue("bucket", entity.BucketStart);
        cmd.Parameters.AddWithValue("model_name", entity.ModelName);
        cmd.Parameters.AddWithValue("model_ver", entity.ModelVersion);
        cmd.Parameters.AddWithValue("score", entity.AnomalyScore);
        cmd.Parameters.AddWithValue("conf", entity.Confidence);
        cmd.Parameters.AddWithValue("per_signal", entity.PerSignalScore);
        cmd.Parameters.AddWithValue("top", entity.TopContributors);
        cmd.Parameters.AddWithValue("created", entity.CreatedAt);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetGuid(0);
    }

    public async Task<IReadOnlyList<object>> GetBySatelliteAsync(Guid satelliteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString))
            return Array.Empty<object>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, satellite_id, bucket_start, model_name, model_version, anomaly_score, confidence, per_signal_score, top_contributors, created_at FROM ml_health_results WHERE satellite_id = @sid AND bucket_start >= @from AND bucket_start <= @to ORDER BY bucket_start DESC",
            conn);
        cmd.Parameters.AddWithValue("sid", satelliteId);
        cmd.Parameters.AddWithValue("from", from);
        cmd.Parameters.AddWithValue("to", to);
        var list = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(new
            {
                Id = reader.GetGuid(0),
                SatelliteId = reader.GetGuid(1),
                BucketStart = reader.GetDateTime(2).ToUniversalTime().ToString("O"),
                ModelName = reader.GetString(3),
                ModelVersion = reader.GetString(4),
                AnomalyScore = reader.GetDouble(5),
                Confidence = reader.GetDouble(6),
                PerSignalScore = reader.GetString(7),
                TopContributors = reader.GetString(8),
                CreatedAt = reader.GetDateTime(9).ToUniversalTime().ToString("O")
            });
        return list;
    }
}

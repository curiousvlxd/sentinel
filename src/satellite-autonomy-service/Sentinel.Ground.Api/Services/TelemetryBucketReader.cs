using Npgsql;
using Sentinel.Core.Contracts;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class TelemetryBucketReader
{
    private readonly string _connectionString;

    public TelemetryBucketReader(IConfiguration configuration)
    {
        var section = configuration.GetSection(GroundOptions.SectionName);
        _connectionString = section["TimescaleConnectionString"] ?? string.Empty;
    }

    public async Task<TelemetryBucketRequest?> GetLatestBucketAsync(Guid satelliteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_connectionString))
            return null;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT bucket, satellite_id,
                   cpu_temperature_mean, cpu_temperature_min, cpu_temperature_max, cpu_temperature_stddev, cpu_temperature_p95, count_per_minute,
                   battery_voltage_mean, battery_voltage_min, battery_voltage_max, battery_voltage_stddev, battery_voltage_p95,
                   pressure_mean, pressure_min, pressure_max, pressure_stddev, pressure_p95,
                   gyro_speed_mean, gyro_speed_min, gyro_speed_max, gyro_speed_stddev, gyro_speed_p95,
                   signal_strength_mean, signal_strength_min, signal_strength_max, signal_strength_stddev, signal_strength_p95,
                   power_consumption_mean, power_consumption_min, power_consumption_max, power_consumption_stddev, power_consumption_p95
            FROM telemetry_1m
            WHERE satellite_id = @sid
            ORDER BY bucket DESC
            LIMIT 1
            """,
            conn);
        cmd.Parameters.AddWithValue("sid", satelliteId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var bucket = reader.GetDateTime(0).ToUniversalTime();
        const int bucketSec = 60;
        const double expectedCount = 60.0;
        var countPerMinute = reader.GetDouble(7);
        var missingRate = 1.0 - Math.Clamp(countPerMinute / expectedCount, 0, 1);

        var req = new TelemetryBucketRequest
        {
            SchemaVersion = "v1",
            SatelliteId = satelliteId,
            BucketStart = bucket.ToString("O"),
            BucketSec = bucketSec,
            Signals = new Dictionary<string, SignalAggregateContract>
            {
                ["cpu_temperature"] = ToSignal(reader, 2, 6, missingRate),
                ["battery_voltage"] = ToSignal(reader, 8, 12, missingRate),
                ["pressure"] = ToSignal(reader, 13, 17, missingRate),
                ["gyro_speed"] = ToSignal(reader, 18, 22, missingRate),
                ["signal_strength"] = ToSignal(reader, 23, 27, missingRate),
                ["power_consumption"] = ToSignal(reader, 28, 32, missingRate)
            }
        };
        return req;
    }

    private static SignalAggregateContract ToSignal(NpgsqlDataReader reader, int meanIdx, int p95Idx, double missingRate)
    {
        var mean = reader.GetDouble(meanIdx);
        var min = reader.GetDouble(meanIdx + 1);
        var max = reader.GetDouble(meanIdx + 2);
        var std = reader.IsDBNull(meanIdx + 3) ? 0 : reader.GetDouble(meanIdx + 3);
        var p95 = reader.GetDouble(p95Idx);
        return new SignalAggregateContract
        {
            Mean = mean,
            Min = min,
            Max = max,
            Std = double.IsNaN(std) ? 0 : std,
            Slope = 0,
            P95 = p95,
            MissingRate = missingRate
        };
    }
}

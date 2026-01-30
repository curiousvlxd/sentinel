using Npgsql;
using Sentinel.Core.Contracts;
using Sentinel.Satellite.Service.Options;

namespace Sentinel.Satellite.Service.Services;

public sealed class TelemetryIngestService
{
    private readonly string _connectionString;

    public TelemetryIngestService(IConfiguration configuration)
    {
        var section = configuration.GetSection(TimescaleOptions.SectionName);
        _connectionString = section["ConnectionString"] ?? string.Empty;
    }

    public async Task<TelemetryIngestResponse> IngestAsync(TelemetryIngestRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_connectionString))
            return new TelemetryIngestResponse { Accepted = false, SatelliteId = request.SatelliteId };

        if (!DateTime.TryParse(request.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ts))
            return new TelemetryIngestResponse { Accepted = false, SatelliteId = request.SatelliteId };

        var storedAt = DateTime.UtcNow;
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO telemetry_points (satellite_id, ts, cpu_temperature, battery_voltage, pressure, gyro_speed, signal_strength, power_consumption, lat, lon, alt_km, source)
            VALUES (@sid, @ts, @cpu, @bat, @press, @gyro, @sig, @power, @lat, @lon, @alt, @source)
            ON CONFLICT (satellite_id, ts) DO NOTHING
            """,
            conn);
        cmd.Parameters.AddWithValue("sid", request.SatelliteId);
        cmd.Parameters.AddWithValue("ts", ts);
        cmd.Parameters.AddWithValue("cpu", request.Signals.CpuTemperature);
        cmd.Parameters.AddWithValue("bat", request.Signals.BatteryVoltage);
        cmd.Parameters.AddWithValue("press", request.Signals.Pressure);
        cmd.Parameters.AddWithValue("gyro", request.Signals.GyroSpeed);
        cmd.Parameters.AddWithValue("sig", request.Signals.SignalStrength);
        cmd.Parameters.AddWithValue("power", request.Signals.PowerConsumption);
        cmd.Parameters.AddWithValue("lat", (object?)request.Location?.Lat ?? DBNull.Value);
        cmd.Parameters.AddWithValue("lon", (object?)request.Location?.Lon ?? DBNull.Value);
        cmd.Parameters.AddWithValue("alt", (object?)request.Location?.AltKm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("source", (object?)request.Source ?? "ingest");
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        return new TelemetryIngestResponse
        {
            Accepted = true,
            StoredAt = storedAt.ToString("O"),
            SatelliteId = request.SatelliteId
        };
    }
}

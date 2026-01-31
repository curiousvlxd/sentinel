using Dapper;
using Npgsql;
using Sentinel.Core.Contracts.Telemetry;
using Sentinel.Satellite.Service.Constants;

namespace Sentinel.Satellite.Service.Services;

public sealed class TelemetryIngestService([FromKeyedServices(DataSources.Satellite)] NpgsqlDataSource dataSource)
{
    private readonly NpgsqlDataSource dataSource = dataSource;

    public async Task<TelemetryIngestResponse> IngestAsync(TelemetryIngestRequest request, CancellationToken cancellationToken)
    {
        var ts = request.Timestamp;
        if (ts == default) return new TelemetryIngestResponse { Accepted = false, SatelliteId = request.SatelliteId };

        var storedAt = DateTimeOffset.UtcNow;
        const string sql = """
            INSERT INTO telemetry_points (satellite_id, ts, cpu_temperature, battery_voltage, pressure, gyro_speed, signal_strength, power_consumption, lat, lon, alt_km, source)
            VALUES (@sid, @ts, @cpu, @bat, @press, @gyro, @sig, @power, @lat, @lon, @alt, @source)
            ON CONFLICT (satellite_id, ts) DO NOTHING
            """;
        await using var conn = (await dataSource.OpenConnectionAsync(cancellationToken));
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            sid = request.SatelliteId,
            ts,
            cpu = request.Signals.CpuTemperature,
            bat = request.Signals.BatteryVoltage,
            press = request.Signals.Pressure,
            gyro = request.Signals.GyroSpeed,
            sig = request.Signals.SignalStrength,
            power = request.Signals.PowerConsumption,
            lat = request.Location?.Lat,
            lon = request.Location?.Lon,
            alt = request.Location?.AltKm,
            source = request.Source ?? "ingest"
        }, cancellationToken: cancellationToken));

        return new TelemetryIngestResponse
        {
            Accepted = true,
            StoredAt = storedAt,
            SatelliteId = request.SatelliteId
        };
    }
}

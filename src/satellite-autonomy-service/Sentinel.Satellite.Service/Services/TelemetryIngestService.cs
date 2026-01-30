using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sentinel.Core.Contracts;

namespace Sentinel.Satellite.Service.Services;

public sealed class TelemetryIngestService
{
    private readonly NpgsqlDataSource _dataSource;

    public TelemetryIngestService([FromKeyedServices("satellite")] NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<TelemetryIngestResponse> IngestAsync(TelemetryIngestRequest request, CancellationToken cancellationToken)
    {
        if (!DateTime.TryParse(request.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ts))
            return new TelemetryIngestResponse { Accepted = false, SatelliteId = request.SatelliteId };

        var storedAt = DateTime.UtcNow;
        const string sql = """
            INSERT INTO telemetry_points (satellite_id, ts, cpu_temperature, battery_voltage, pressure, gyro_speed, signal_strength, power_consumption, lat, lon, alt_km, source)
            VALUES (@sid, @ts, @cpu, @bat, @press, @gyro, @sig, @power, @lat, @lon, @alt, @source)
            ON CONFLICT (satellite_id, ts) DO NOTHING
            """;
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
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
            StoredAt = storedAt.ToString("O"),
            SatelliteId = request.SatelliteId
        };
    }
}

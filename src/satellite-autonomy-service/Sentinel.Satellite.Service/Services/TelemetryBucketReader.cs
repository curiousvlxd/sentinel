using Dapper;
using Npgsql;
using Sentinel.Core.Contracts;

namespace Sentinel.Satellite.Service.Services;

public sealed class TelemetryBucketReader
{
    private readonly NpgsqlDataSource _dataSource;

    public TelemetryBucketReader(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<TelemetryBucketRequest?> GetLatestBucketAsync(Guid satelliteId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        const string sql =
            """
            SELECT bucket AS Bucket, satellite_id AS SatelliteId,
                   cpu_temperature_mean AS CpuTemperatureMean, cpu_temperature_min AS CpuTemperatureMin, cpu_temperature_max AS CpuTemperatureMax,
                   cpu_temperature_stddev AS CpuTemperatureStddev, cpu_temperature_p95 AS CpuTemperatureP95, count_per_minute AS CountPerMinute,
                   battery_voltage_mean AS BatteryVoltageMean, battery_voltage_min AS BatteryVoltageMin, battery_voltage_max AS BatteryVoltageMax,
                   battery_voltage_stddev AS BatteryVoltageStddev, battery_voltage_p95 AS BatteryVoltageP95,
                   pressure_mean AS PressureMean, pressure_min AS PressureMin, pressure_max AS PressureMax,
                   pressure_stddev AS PressureStddev, pressure_p95 AS PressureP95,
                   gyro_speed_mean AS GyroSpeedMean, gyro_speed_min AS GyroSpeedMin, gyro_speed_max AS GyroSpeedMax,
                   gyro_speed_stddev AS GyroSpeedStddev, gyro_speed_p95 AS GyroSpeedP95,
                   signal_strength_mean AS SignalStrengthMean, signal_strength_min AS SignalStrengthMin, signal_strength_max AS SignalStrengthMax,
                   signal_strength_stddev AS SignalStrengthStddev, signal_strength_p95 AS SignalStrengthP95,
                   power_consumption_mean AS PowerConsumptionMean, power_consumption_min AS PowerConsumptionMin, power_consumption_max AS PowerConsumptionMax,
                   power_consumption_stddev AS PowerConsumptionStddev, power_consumption_p95 AS PowerConsumptionP95
            FROM telemetry_1m
            WHERE satellite_id = @sid
            ORDER BY bucket DESC
            LIMIT 1
            """;
        var row = await conn.QuerySingleOrDefaultAsync<Telemetry1mRow>(new CommandDefinition(sql, new { sid = satelliteId }, cancellationToken: cancellationToken));
        if (row == null) return null;

        const double expectedCount = 60.0;
        var missingRate = 1.0 - Math.Clamp(row.CountPerMinute / expectedCount, 0, 1);
        var bucket = row.Bucket.Kind == DateTimeKind.Utc ? row.Bucket : DateTime.SpecifyKind(row.Bucket, DateTimeKind.Utc);

        return new TelemetryBucketRequest
        {
            SchemaVersion = "v1",
            SatelliteId = satelliteId,
            BucketStart = bucket.ToString("O"),
            BucketSec = 60,
            Signals = new Dictionary<string, SignalAggregateContract>
            {
                ["cpu_temperature"] = ToSignal(row.CpuTemperatureMean, row.CpuTemperatureMin, row.CpuTemperatureMax, row.CpuTemperatureStddev, row.CpuTemperatureP95, missingRate),
                ["battery_voltage"] = ToSignal(row.BatteryVoltageMean, row.BatteryVoltageMin, row.BatteryVoltageMax, row.BatteryVoltageStddev, row.BatteryVoltageP95, missingRate),
                ["pressure"] = ToSignal(row.PressureMean, row.PressureMin, row.PressureMax, row.PressureStddev, row.PressureP95, missingRate),
                ["gyro_speed"] = ToSignal(row.GyroSpeedMean, row.GyroSpeedMin, row.GyroSpeedMax, row.GyroSpeedStddev, row.GyroSpeedP95, missingRate),
                ["signal_strength"] = ToSignal(row.SignalStrengthMean, row.SignalStrengthMin, row.SignalStrengthMax, row.SignalStrengthStddev, row.SignalStrengthP95, missingRate),
                ["power_consumption"] = ToSignal(row.PowerConsumptionMean, row.PowerConsumptionMin, row.PowerConsumptionMax, row.PowerConsumptionStddev, row.PowerConsumptionP95, missingRate)
            }
        };
    }

    private static SignalAggregateContract ToSignal(double mean, double min, double max, double? std, double p95, double missingRate) =>
        new()
        {
            Mean = mean,
            Min = min,
            Max = max,
            Std = (std.HasValue && !double.IsNaN(std.Value)) ? std.Value : 0,
            Slope = 0,
            P95 = p95,
            MissingRate = missingRate
        };

    private sealed class Telemetry1mRow
    {
        public DateTime Bucket { get; init; }
        public Guid SatelliteId { get; init; }
        public double CpuTemperatureMean { get; init; }
        public double CpuTemperatureMin { get; init; }
        public double CpuTemperatureMax { get; init; }
        public double? CpuTemperatureStddev { get; init; }
        public double CpuTemperatureP95 { get; init; }
        public double CountPerMinute { get; init; }
        public double BatteryVoltageMean { get; init; }
        public double BatteryVoltageMin { get; init; }
        public double BatteryVoltageMax { get; init; }
        public double? BatteryVoltageStddev { get; init; }
        public double BatteryVoltageP95 { get; init; }
        public double PressureMean { get; init; }
        public double PressureMin { get; init; }
        public double PressureMax { get; init; }
        public double? PressureStddev { get; init; }
        public double PressureP95 { get; init; }
        public double GyroSpeedMean { get; init; }
        public double GyroSpeedMin { get; init; }
        public double GyroSpeedMax { get; init; }
        public double? GyroSpeedStddev { get; init; }
        public double GyroSpeedP95 { get; init; }
        public double SignalStrengthMean { get; init; }
        public double SignalStrengthMin { get; init; }
        public double SignalStrengthMax { get; init; }
        public double? SignalStrengthStddev { get; init; }
        public double SignalStrengthP95 { get; init; }
        public double PowerConsumptionMean { get; init; }
        public double PowerConsumptionMin { get; init; }
        public double PowerConsumptionMax { get; init; }
        public double? PowerConsumptionStddev { get; init; }
        public double PowerConsumptionP95 { get; init; }
    }
}

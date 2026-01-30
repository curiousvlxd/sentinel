CREATE MATERIALIZED VIEW IF NOT EXISTS telemetry_1m
WITH (timescaledb.continuous) AS
SELECT
    time_bucket(INTERVAL '1 minute', ts) AS bucket,
    satellite_id,
    AVG(cpu_temperature)    AS cpu_temperature_mean,
    MIN(cpu_temperature)    AS cpu_temperature_min,
    MAX(cpu_temperature)    AS cpu_temperature_max,
    STDDEV(cpu_temperature) AS cpu_temperature_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY cpu_temperature) AS cpu_temperature_p95,
    AVG(battery_voltage)     AS battery_voltage_mean,
    MIN(battery_voltage)    AS battery_voltage_min,
    MAX(battery_voltage)    AS battery_voltage_max,
    STDDEV(battery_voltage) AS battery_voltage_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY battery_voltage) AS battery_voltage_p95,
    AVG(pressure)           AS pressure_mean,
    MIN(pressure)            AS pressure_min,
    MAX(pressure)            AS pressure_max,
    STDDEV(pressure)        AS pressure_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY pressure) AS pressure_p95,
    AVG(gyro_speed)         AS gyro_speed_mean,
    MIN(gyro_speed)         AS gyro_speed_min,
    MAX(gyro_speed)         AS gyro_speed_max,
    STDDEV(gyro_speed)      AS gyro_speed_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY gyro_speed) AS gyro_speed_p95,
    AVG(signal_strength)    AS signal_strength_mean,
    MIN(signal_strength)    AS signal_strength_min,
    MAX(signal_strength)    AS signal_strength_max,
    STDDEV(signal_strength) AS signal_strength_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY signal_strength) AS signal_strength_p95,
    AVG(power_consumption)  AS power_consumption_mean,
    MIN(power_consumption)  AS power_consumption_min,
    MAX(power_consumption)  AS power_consumption_max,
    STDDEV(power_consumption) AS power_consumption_stddev,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY power_consumption) AS power_consumption_p95,
    COUNT(*)::double precision / 60.0 AS count_per_minute
FROM telemetry_points
GROUP BY 1, 2
WITH NO DATA;

CREATE INDEX IF NOT EXISTS ix_telemetry_1m_satellite_bucket
    ON telemetry_1m (satellite_id, bucket DESC);

SELECT add_continuous_aggregate_policy('telemetry_1m', null, null, schedule_interval => INTERVAL '1 minute');

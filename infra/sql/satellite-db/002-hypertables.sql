CREATE TABLE IF NOT EXISTS telemetry_points
(
    satellite_id       uuid         NOT NULL,
    ts                 timestamptz  NOT NULL,
    cpu_temperature    double precision,
    battery_voltage    double precision,
    pressure           double precision,
    gyro_speed         double precision,
    signal_strength    double precision,
    power_consumption  double precision,
    lat                double precision NULL,
    lon                double precision NULL,
    alt_km             double precision NULL,
    source             text NULL,
    PRIMARY KEY (satellite_id, ts)
);

SELECT create_hypertable('telemetry_points', 'ts', if_not_exists => TRUE);

CREATE INDEX IF NOT EXISTS ix_telemetry_points_satellite_ts
    ON telemetry_points (satellite_id, ts DESC);

SELECT add_retention_policy('telemetry_points', INTERVAL '1 day', if_not_exists => TRUE);

SELECT add_retention_policy('telemetry_1m', INTERVAL '7 days', if_not_exists => TRUE);

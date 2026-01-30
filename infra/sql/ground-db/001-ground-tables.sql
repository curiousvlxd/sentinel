CREATE TABLE IF NOT EXISTS missions
(
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name        text NOT NULL,
    description text,
    created_at  timestamptz NOT NULL DEFAULT now(),
    is_active   boolean NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS satellites
(
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    mission_id  uuid NOT NULL REFERENCES missions(id),
    name        text NOT NULL,
    norad_id    int NULL,
    external_id text NULL,
    status      text NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Standby', 'Lost')),
    created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_satellites_mission_id ON satellites(mission_id);
CREATE INDEX IF NOT EXISTS ix_satellites_status ON satellites(status);

CREATE TABLE IF NOT EXISTS ml_health_results
(
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    satellite_id      uuid NOT NULL,
    bucket_start      timestamptz NOT NULL,
    model_name        text NOT NULL,
    model_version     text NOT NULL,
    anomaly_score     double precision NOT NULL,
    confidence        double precision NOT NULL,
    per_signal_score  jsonb NOT NULL DEFAULT '{}',
    top_contributors  jsonb NOT NULL DEFAULT '[]',
    created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_ml_health_results_satellite_bucket
    ON ml_health_results(satellite_id, bucket_start DESC);

CREATE TABLE IF NOT EXISTS decisions
(
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    satellite_id  uuid NOT NULL,
    bucket_start  timestamptz NOT NULL,
    decision_type text NOT NULL CHECK (decision_type IN ('None', 'ThrottlePayload', 'ReducePower', 'SwitchMode', 'RaiseAlert')),
    reason        text NOT NULL,
    created_at    timestamptz NOT NULL DEFAULT now(),
    metadata      jsonb NULL
);

CREATE INDEX IF NOT EXISTS ix_decisions_satellite_bucket
    ON decisions(satellite_id, bucket_start DESC);

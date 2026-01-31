from __future__ import annotations

from datetime import datetime, timezone

from fastapi import FastAPI, HTTPException, Query
from pydantic import BaseModel, Field

import numpy as np

from .config import SETTINGS
from .model import load_artifact, load_train_stats, score_bucket
from .schemas import (
    TelemetryBucketRequest,
    TelemetryBucketResponse,
    MLBlock,
    ModelInfo,
    Contributor,
)
from .simulate import SimScenario, generate_bucket_for_scenario, set_command_effect

app = FastAPI(title="Telemetry Anomaly Scoring", version="1.0")


class SimulationCommandRequest(BaseModel):
    commandType: str = Field(default="", alias="commandType")


class SimulateScoreRequest(BaseModel):
    scenario: SimScenario = Field(default="Normal", description="Normal | Mixed | Anomaly")
    bucket_start: str | None = Field(default=None, description="ISO datetime; default now")
    seed: int | None = Field(default=None, description="RNG seed for reproducibility")

_artifact = None
_stats = None


@app.on_event("startup")
def _startup() -> None:
    global _artifact, _stats
    try:
        _artifact = load_artifact(SETTINGS.model_path)
        _stats = load_train_stats(SETTINGS.train_stats_path)
        print(f"Loaded model artifact from {SETTINGS.model_path}")
        print(f"Loaded train stats from {SETTINGS.train_stats_path}")
    except Exception as e:
        _artifact = None
        _stats = None
        print(f"WARNING: Failed to load artifacts: {e}")


@app.post("/simulation/command")
def simulation_command(body: SimulationCommandRequest) -> None:
    set_command_effect(body.commandType or "")


@app.get("/sample", response_model=TelemetryBucketRequest)
def get_sample(
    scenario: SimScenario = Query(default="Normal", description="Normal | Mixed | Anomaly"),
    seed: int | None = Query(default=None, description="RNG seed"),
) -> TelemetryBucketRequest:
    """Generate a sample telemetry bucket for the given scenario (demo/test data)."""
    rng = np.random.default_rng(seed)
    bucket_start = datetime.now(timezone.utc)
    return generate_bucket_for_scenario(bucket_start, rng, scenario)


def _mock_ml_response(
    scenario: SimScenario,
    bucket_start: str,
    signal_names: list[str],
) -> TelemetryBucketResponse:
    if scenario == "Normal":
        anomaly_score, confidence = 0.08, 0.92
        per_signal_base = 0.05
        top_n = 2
    elif scenario == "Mixed":
        anomaly_score, confidence = 0.45, 0.70
        per_signal_base = 0.35
        top_n = 3
    else:
        anomaly_score, confidence = 0.88, 0.90
        per_signal_base = 0.75
        top_n = 3
    rng = np.random.default_rng()
    per_signal = {
        name: round(per_signal_base * (0.85 + 0.3 * rng.random()), 4)
        for name in signal_names
    }
    keys = [f"{name}.missing_rate" for name in signal_names[:top_n]]
    weights = [0.5 - 0.1 * i for i in range(len(keys))]
    total = sum(weights)
    top = [Contributor(key=k, weight=round(w / total, 3)) for k, w in zip(keys, weights)]
    return TelemetryBucketResponse(
        schema_version="v1",
        bucket_start=bucket_start,
        ml=MLBlock(
            model=ModelInfo(name="isolation_forest", version="v1.0"),
            anomaly_score=round(anomaly_score, 4),
            confidence=round(confidence, 2),
            per_signal_score=per_signal,
            top_contributors=top,
        ),
    )


@app.post("/score/simulate", response_model=TelemetryBucketResponse)
def score_simulate(body: SimulateScoreRequest) -> TelemetryBucketResponse:
    """Generate a bucket for the scenario and return ML score (real model if loaded, else mock)."""
    rng = np.random.default_rng(body.seed)
    if body.bucket_start:
        try:
            bucket_dt = datetime.fromisoformat(body.bucket_start.replace("Z", "+00:00"))
        except ValueError:
            bucket_dt = datetime.now(timezone.utc)
    else:
        bucket_dt = datetime.now(timezone.utc)
    req = generate_bucket_for_scenario(bucket_dt, rng, body.scenario)
    bucket_start_str = req.bucket_start
    signal_names = list(req.signals.keys())

    if _artifact is not None and _stats is not None:
        anomaly_score, confidence, per_signal, top = score_bucket(req, _artifact, _stats, top_n=3)
        return TelemetryBucketResponse(
            schema_version=req.schema_version,
            bucket_start=bucket_start_str,
            ml=MLBlock(
                model=ModelInfo(name="isolation_forest", version="v1.0"),
                anomaly_score=round(anomaly_score, 4),
                confidence=round(confidence, 2),
                per_signal_score={k: round(float(v), 4) for k, v in per_signal.items()},
                top_contributors=[Contributor(key=k, weight=round(w, 4)) for k, w in top],
            ),
        )
    return _mock_ml_response(body.scenario, bucket_start_str, signal_names)


@app.post("/score", response_model=TelemetryBucketResponse)
def score(req: TelemetryBucketRequest) -> TelemetryBucketResponse:
    if _artifact is None or _stats is None:
        raise HTTPException(status_code=503, detail="Model artifacts not loaded. Train first.")

    anomaly_score, confidence, per_signal, top = score_bucket(req, _artifact, _stats, top_n=3)

    return TelemetryBucketResponse(
        schema_version=req.schema_version,
        bucket_start=req.bucket_start,
        ml=MLBlock(
            model=ModelInfo(name="isolation_forest", version="v1.0"),
            anomaly_score=anomaly_score,
            confidence=confidence,
            per_signal_score={k: float(v) for k, v in per_signal.items()},
            top_contributors=[Contributor(key=k, weight=w) for k, w in top],
        ),
    )

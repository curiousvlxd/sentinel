from __future__ import annotations

from typing import Dict, List
from pydantic import BaseModel, Field, ConfigDict, model_validator


class SignalFeatures(BaseModel):
    mean: float
    min: float
    max: float
    std: float
    slope: float
    p95: float
    missing_rate: float


class TelemetryBucketRequest(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    schema_version: str = "v1"
    bucket_start: str
    bucket_sec: int = 60

    signals: Dict[str, SignalFeatures] = Field(default_factory=dict, alias="features")

    @model_validator(mode="before")
    @classmethod
    def merge_signals_and_features(cls, data):
        if not isinstance(data, dict):
            return data

        signals = data.get("signals")
        features = data.get("features")

        if features is None and isinstance(signals, dict):
            data["features"] = signals
            return data

        if isinstance(signals, dict) and isinstance(features, dict):
            merged = dict(signals)
            merged.update(features)
            data["features"] = merged

        return data


class ModelInfo(BaseModel):
    name: str = "isolation_forest"
    version: str = "v1.0"


class Contributor(BaseModel):
    key: str
    weight: float


class MLBlock(BaseModel):
    model: ModelInfo = Field(default_factory=ModelInfo)

    anomaly_score: float
    confidence: float

    per_signal_score: Dict[str, float] = Field(default_factory=dict)
    top_contributors: List[Contributor] = Field(default_factory=list)


class TelemetryBucketResponse(BaseModel):
    schema_version: str = "v1"
    bucket_start: str
    ml: MLBlock


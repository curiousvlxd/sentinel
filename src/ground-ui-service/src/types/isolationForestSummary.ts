export interface IsolationForestContributor {
  key: string;
  weight: number;
}

export interface IsolationForestML {
  model: {
    name: string;
    version: string;
  };
  anomaly_score: number;
  confidence: number;
  per_signal_score: Record<string, number>;
  top_contributors: IsolationForestContributor[];
}

export interface IsolationForestSummary {
  schema_version: string;
  bucket_start: string; // ISO timestamp
  ml: IsolationForestML;
  ai_response?: string | null; // nullable AI made response
}

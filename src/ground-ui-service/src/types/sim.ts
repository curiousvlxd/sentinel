export type SimScenario = "Normal" | "Mixed" | "Anomaly";

export interface SimStartRequest {
  scenario: SimScenario;
}

import type { IsolationForestSummary } from "./isolationForestSummary";

export interface Satellite {
  id: string;
  name: string;
  description?: string;
  isolationForestSummary: IsolationForestSummary[];

  launchDate?: string;
  launchVehicle?: string;
  launchSite?: string;

  orbitType?: "LEO" | "MEO" | "GEO" | "HEO" | string;
  altitudeKm?: number;
  inclinationDeg?: number;
  periodMin?: number;

  status?: "active" | "inactive" | "decommissioned" | "lost";
  operator?: string;
  country?: string;

  massKg?: number;
  dimensionsM?: { length: number; width: number; height: number };

  frequencyBands?: string[];
  transponderCount?: number;

  sensors?: string[];
  resolutionM?: number;

  tags?: string[];
  website?: string;
}

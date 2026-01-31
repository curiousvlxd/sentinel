import type { IsolationForestSummary } from "./isolationForestSummary";

export interface Satellite {
  id: string;
  name: string;
  description?: string;
  isolationForestSummary: IsolationForestSummary[];

  // Launch details
  launchDate?: string; // ISO date string
  launchVehicle?: string; // e.g., "Falcon 9"
  launchSite?: string; // e.g., "Cape Canaveral"

  // Orbital parameters
  orbitType?: "LEO" | "MEO" | "GEO" | "HEO" | string; // Low/Mid/Geo/High Earth Orbit
  altitudeKm?: number; // Approximate altitude in kilometers
  inclinationDeg?: number; // Orbital inclination
  periodMin?: number; // Orbital period in minutes

  // Operational status
  status?: "active" | "inactive" | "decommissioned" | "lost";
  operator?: string; // Organization operating the satellite
  country?: string; // Country of origin

  // Physical characteristics
  massKg?: number;
  dimensionsM?: { length: number; width: number; height: number };

  // Communication
  frequencyBands?: string[]; // e.g., ["S-band", "X-band"]
  transponderCount?: number;

  // Imaging / sensors
  sensors?: string[]; // e.g., ["optical camera", "radar"]
  resolutionM?: number; // Ground resolution in meters

  // Optional metadata
  tags?: string[];
  website?: string;
}

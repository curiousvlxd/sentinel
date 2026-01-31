import type { IsolationForestSummary } from "@/types/isolationForestSummary";
import type { Satellite } from "@/types/satellite";

export class ApiService {
  private static mockIsolationForest: IsolationForestSummary[] = [
    {
      schema_version: "v1",
      bucket_start: "2026-01-29T22:32:00Z",
      ml: {
        model: { name: "isolation_forest", version: "v1.0" },
        anomaly_score: 0.1328,
        confidence: 0.7344,
        per_signal_score: {
          battery_voltage: 0.6332,
          cpu_temperature: 0.2902,
          gyro_speed: 0.3346,
          power_consumption: 0.3496,
          pressure: 0.33,
          signal_strength: 0.2899,
        },
        top_contributors: [
          { key: "battery_voltage.min", weight: 0.4807 },
          { key: "power_consumption.p95", weight: 0.2654 },
          { key: "gyro_speed.p95", weight: 0.254 },
        ],
      },
      ai_response: null,
    },
    {
      schema_version: "v1",
      bucket_start: "2026-01-30T01:12:00Z",
      ml: {
        model: { name: "isolation_forest", version: "v1.0" },
        anomaly_score: 0.8756,
        confidence: 0.9123,
        per_signal_score: {
          battery_voltage: 0.912,
          cpu_temperature: 0.755,
          gyro_speed: 0.845,
          power_consumption: 0.9,
          pressure: 0.65,
          signal_strength: 0.8,
        },
        top_contributors: [
          { key: "power_consumption.p95", weight: 0.4 },
          { key: "gyro_speed.max", weight: 0.35 },
          { key: "battery_voltage.max", weight: 0.25 },
        ],
      },
      ai_response:
        "Battery voltage anomaly detected, likely due to load spike.",
    },
  ];

  private static mockSatellites: Satellite[] = [
    {
      id: "s1",
      name: "Hubble",
      description: "Space telescope observing distant stars and galaxies",
      isolationForestSummary: ApiService.mockIsolationForest,
      launchDate: "1990-04-24",
      launchVehicle: "Space Shuttle Discovery",
      launchSite: "Kennedy Space Center",
      orbitType: "LEO",
      altitudeKm: 547,
      inclinationDeg: 28.5,
      periodMin: 96,
      status: "active",
      operator: "NASA / ESA",
      country: "USA",
      massKg: 11110,
      dimensionsM: { length: 13.2, width: 4.2, height: 4.2 },
      frequencyBands: ["S-band", "X-band"],
      transponderCount: 2,
      sensors: ["Optical telescope", "Spectrograph"],
      resolutionM: 0.05,
      tags: ["astronomy", "telescope", "space"],
      website: "https://www.nasa.gov/mission_pages/hubble/main/index.html",
    },
    {
      id: "s2",
      name: "ISS",
      description: "International Space Station orbiting Earth",
      isolationForestSummary: ApiService.mockIsolationForest,
      launchDate: "1998-11-20",
      launchVehicle: "Proton, Space Shuttle",
      launchSite: "Baikonur Cosmodrome",
      orbitType: "LEO",
      altitudeKm: 408,
      inclinationDeg: 51.6,
      periodMin: 92,
      status: "active",
      operator: "NASA / Roscosmos / ESA / JAXA / CSA",
      country: "International",
      massKg: 419725,
      dimensionsM: { length: 72.8, width: 108.5, height: 20 },
      frequencyBands: ["S-band", "Ku-band", "Ka-band"],
      transponderCount: 8,
      sensors: ["Life support", "Earth observation modules"],
      resolutionM: 0.5,
      tags: ["research", "habitat", "international"],
      website: "https://www.nasa.gov/mission_pages/station/main/index.html",
    },
    {
      id: "s3",
      name: "GPS IIF-1",
      description: "Navigation satellite in medium Earth orbit",
      isolationForestSummary: ApiService.mockIsolationForest,
      launchDate: "2010-05-27",
      launchVehicle: "Delta IV",
      launchSite: "Cape Canaveral",
      orbitType: "MEO",
      altitudeKm: 20180,
      inclinationDeg: 55,
      periodMin: 720,
      status: "active",
      operator: "USAF",
      country: "USA",
      massKg: 1630,
      dimensionsM: { length: 5, width: 2.7, height: 1.5 },
      frequencyBands: ["L1", "L2", "L5"],
      transponderCount: 3,
      sensors: ["Atomic clock", "Navigation payload"],
      resolutionM: undefined,
      tags: ["navigation", "GPS", "MEO"],
      website: "https://www.gps.gov/systems/gps/space/launches/",
    },
  ];

  static async getSatellites(): Promise<Satellite[]> {
    return this.mockSatellites;
  }

  static async getSatelliteById(id: string): Promise<Satellite | undefined> {
    return this.mockSatellites.find((sat) => sat.id === id);
  }
}

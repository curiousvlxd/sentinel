export interface MlTopContributorItem {
  key: string;
  weight: number;
}

export interface SatelliteMlResultItem {
  id: string;
  satelliteId: string;
  bucketStart: string;
  modelName: string;
  modelVersion: string;
  anomalyScore: number;
  confidence: number;
  perSignalScore: Record<string, number>;
  topContributors: MlTopContributorItem[];
  createdAt: string;
}

export interface SatelliteMlResultsResponse {
  response: SatelliteMlResultItem[];
}

export type MlResultDto = SatelliteMlResultItem;

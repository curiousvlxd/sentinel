export interface MlResultDto {
  id: string;
  satelliteId: string;
  bucketStart: string;
  modelName?: string;
  modelVersion?: string;
  anomalyScore: number;
  confidence: number;
  perSignalScore?: Record<string, number>;
  topContributors?: Array<{ key: string; weight: number }>;
  createdAt: string;
}

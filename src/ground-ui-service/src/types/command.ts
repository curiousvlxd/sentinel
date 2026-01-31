export interface CommandDto {
  id: string;
  satelliteId: string;
  missionId?: string | null;
  type: string;
  payloadJson?: string | null;
  priority: number;
  ttlSec: number;
  status: string;
  createdAt: string;
  claimedAt?: string | null;
  executedAt?: string | null;
}

export interface CommandCreateRequest {
  type: string;
  priority: number;
  ttlSec: number;
  payloadJson?: string | null;
}

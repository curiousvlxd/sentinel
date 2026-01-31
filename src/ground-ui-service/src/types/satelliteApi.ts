/** API shape for satellite (Ground.Api) */
export interface SatelliteDto {
  id: string;
  missionId?: string | null;
  missionName?: string | null;
  name: string;
  status: string;
  mode: string;
  state: string;
  linkStatus: string;
  lastBucketStart?: string | null;
  createdAt: string;
}

export interface SatelliteCreateRequest {
  name: string;
  mode?: string;
}

export interface SatelliteUpdateRequest {
  name: string;
  status: string;
  mode: string;
  state: string;
  linkStatus: string;
}

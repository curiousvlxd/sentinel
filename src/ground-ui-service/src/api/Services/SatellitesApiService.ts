import apiClient from "@/api/client";
import { createResourceApi } from "@/api/createResourceApi";
import type { DecisionDto } from "@/types/decision";
import type { SatelliteMlResultsResponse } from "@/types/mlResult";
import type {
  SatelliteDto,
  SatelliteCreateRequest,
  SatelliteUpdateRequest,
} from "@/types/satelliteApi";

const satellitesApi = createResourceApi<
  SatelliteDto,
  SatelliteCreateRequest,
  SatelliteUpdateRequest
>("satellites");

export const SatellitesApiService = {
  ...satellitesApi,
  list: (missionId?: string) =>
    satellitesApi.list(missionId ? { missionId } : undefined),
  getDecisions: (
    satelliteId: string,
    from?: string,
    to?: string,
    limit = 100
  ) =>
    apiClient
      .get<DecisionDto[]>(`satellites/${satelliteId}/decisions`, {
        params: { limit, ...(from && { from }), ...(to && { to }) },
      })
      .then((r) => r.data),
  getMlResults: (satelliteId: string, from?: string, to?: string) =>
    apiClient
      .get<SatelliteMlResultsResponse>(`satellites/${satelliteId}/ml-results`, {
        params: { ...(from && { from }), ...(to && { to }) },
      })
      .then((r) => r.data),
};

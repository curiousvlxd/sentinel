import apiClient from "@/api/client";
import { createResourceApi } from "@/api/createResourceApi";
import type {
  Mission,
  MissionCreateRequest,
  MissionUpdateRequest,
} from "@/types/mission";

const missionsApi = createResourceApi<Mission, MissionCreateRequest, MissionUpdateRequest>("missions");

export const MissionsApiService = {
  ...missionsApi,
  attachSatellite: (missionId: string, satelliteId: string) =>
    apiClient.post(`missions/${missionId}/satellites/${satelliteId}`),
  detachSatellite: (missionId: string, satelliteId: string) =>
    apiClient.delete(`missions/${missionId}/satellites/${satelliteId}`),
};

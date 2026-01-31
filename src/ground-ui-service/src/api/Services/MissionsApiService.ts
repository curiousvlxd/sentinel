import apiClient from "@/api/client";
import type {
  Mission,
  MissionCreateRequest,
  MissionUpdateRequest,
} from "@/types/mission";

const base = "missions";

export const MissionsApiService = {
  list: async (): Promise<Mission[]> => {
    const { data } = await apiClient.get<Mission[]>(base);
    return data;
  },

  get: async (missionId: string): Promise<Mission> => {
    const { data } = await apiClient.get<Mission>(`${base}/${missionId}`);
    return data;
  },

  create: async (body: MissionCreateRequest): Promise<Mission> => {
    const { data } = await apiClient.post<Mission>(base, body);
    return data;
  },

  update: async (
    missionId: string,
    body: MissionUpdateRequest
  ): Promise<Mission> => {
    const { data } = await apiClient.put<Mission>(`${base}/${missionId}`, body);
    return data;
  },

  delete: async (missionId: string): Promise<void> => {
    await apiClient.delete(`${base}/${missionId}`);
  },

  attachSatellite: async (
    missionId: string,
    satelliteId: string
  ): Promise<void> => {
    await apiClient.post(
      `${base}/${missionId}/satellites/${satelliteId}`
    );
  },

  detachSatellite: async (
    missionId: string,
    satelliteId: string
  ): Promise<void> => {
    await apiClient.delete(
      `${base}/${missionId}/satellites/${satelliteId}`
    );
  },
};

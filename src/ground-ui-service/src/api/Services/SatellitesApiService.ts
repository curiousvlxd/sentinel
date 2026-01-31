import apiClient from "@/api/client";
import type {
  SatelliteDto,
  SatelliteCreateRequest,
  SatelliteUpdateRequest,
} from "@/types/satelliteApi";
import type { DecisionDto } from "@/types/decision";
import type { MlResultDto } from "@/types/mlResult";

const base = "satellites";

export const SatellitesApiService = {
  list: async (missionId?: string): Promise<SatelliteDto[]> => {
    const params = missionId ? { missionId } : {};
    const { data } = await apiClient.get<SatelliteDto[]>(base, { params });
    return data;
  },

  get: async (satelliteId: string): Promise<SatelliteDto> => {
    const { data } = await apiClient.get<SatelliteDto>(`${base}/${satelliteId}`);
    return data;
  },

  create: async (body: SatelliteCreateRequest): Promise<SatelliteDto> => {
    const { data } = await apiClient.post<SatelliteDto>(base, body);
    return data;
  },

  update: async (
    satelliteId: string,
    body: SatelliteUpdateRequest
  ): Promise<SatelliteDto> => {
    const { data } = await apiClient.put<SatelliteDto>(
      `${base}/${satelliteId}`,
      body
    );
    return data;
  },

  delete: async (satelliteId: string): Promise<void> => {
    await apiClient.delete(`${base}/${satelliteId}`);
  },

  getDecisions: async (
    satelliteId: string,
    from?: string,
    to?: string,
    limit = 100
  ): Promise<DecisionDto[]> => {
    const params: Record<string, string | number> = { limit };
    if (from) params.from = from;
    if (to) params.to = to;
    const { data } = await apiClient.get<DecisionDto[]>(
      `${base}/${satelliteId}/decisions`,
      { params }
    );
    return data;
  },

  getMlResults: async (
    satelliteId: string,
    from?: string,
    to?: string
  ): Promise<MlResultDto[]> => {
    const params: Record<string, string> = {};
    if (from) params.from = from;
    if (to) params.to = to;
    const { data } = await apiClient.get<MlResultDto[]>(
      `${base}/${satelliteId}/ml-results`,
      { params }
    );
    return data;
  },
};

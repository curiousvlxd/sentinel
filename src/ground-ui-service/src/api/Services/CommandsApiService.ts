import apiClient from "@/api/client";
import type { CommandDto, CommandCreateRequest } from "@/types/command";

export const CommandsApiService = {
  listByMission: async (
    missionId: string,
    status?: string
  ): Promise<CommandDto[]> => {
    const params = status ? { status } : {};
    const { data } = await apiClient.get<CommandDto[]>(
      `missions/${missionId}/commands`,
      { params }
    );
    return data;
  },

  listBySatellite: async (
    satelliteId: string,
    status?: string
  ): Promise<CommandDto[]> => {
    const params = status ? { status } : {};
    const { data } = await apiClient.get<CommandDto[]>(
      `satellites/${satelliteId}/commands`,
      { params }
    );
    return data;
  },

  get: async (commandId: string): Promise<CommandDto> => {
    const { data } = await apiClient.get<CommandDto>(`commands/${commandId}`);
    return data;
  },

  create: async (
    satelliteId: string,
    body: CommandCreateRequest
  ): Promise<CommandDto> => {
    const { data } = await apiClient.post<CommandDto>(
      `satellites/${satelliteId}/commands`,
      body
    );
    return data;
  },
};

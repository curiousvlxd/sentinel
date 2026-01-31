import apiClient from "@/api/client";
import type {
  CommandDto,
  CommandCreateRequest,
  CommandTemplateDto,
  CommandTemplateCreateRequest,
  CommandTemplateUpdateRequest,
} from "@/types/command";

export const CommandsApiService = {
  getTemplates: async (): Promise<CommandTemplateDto[]> => {
    const { data } = await apiClient.get<CommandTemplateDto[]>("commands/templates");
    return data;
  },

  getTemplate: async (id: string): Promise<CommandTemplateDto> => {
    const { data } = await apiClient.get<CommandTemplateDto>(`commands/templates/${id}`);
    return data;
  },

  createTemplate: async (body: CommandTemplateCreateRequest): Promise<CommandTemplateDto> => {
    const { data } = await apiClient.post<CommandTemplateDto>("commands/templates", body);
    return data;
  },

  updateTemplate: async (id: string, body: CommandTemplateUpdateRequest): Promise<CommandTemplateDto> => {
    const { data } = await apiClient.put<CommandTemplateDto>(`commands/templates/${id}`, body);
    return data;
  },

  deleteTemplate: async (id: string): Promise<void> => {
    await apiClient.delete(`commands/templates/${id}`);
  },

  execute: async (commandId: string): Promise<void> => {
    await apiClient.post(`commands/${commandId}/execute`);
  },

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

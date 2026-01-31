import apiClient from "@/api/client";
import type { DecisionDto } from "@/types/decision";

export const DecisionsApiService = {
  get: async (decisionId: string): Promise<DecisionDto> => {
    const { data } = await apiClient.get<DecisionDto>(
      `decisions/${decisionId}`
    );
    return data;
  },
};

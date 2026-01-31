export interface Mission {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface MissionCreateRequest {
  name: string;
  description?: string | null;
}

export interface MissionUpdateRequest {
  name: string;
  description?: string | null;
  isActive: boolean;
}

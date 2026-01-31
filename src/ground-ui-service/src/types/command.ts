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
  commandTemplateId?: string | null;
  priority: number;
  ttlSec: number;
  payloadJson?: string | null;
}

export interface PayloadFieldDto {
  id?: string;
  name: string;
  fieldType: string;
  unit?: number | null;
  default?: unknown;
}

export interface CommandTemplateDto {
  id: string;
  type: string;
  description: string;
  createdAt?: string;
  payloadSchema: PayloadFieldDto[];
}

export interface PayloadFieldCreateDto {
  name: string;
  fieldType: string;
  unit: number;
  defaultValue?: string | null;
}

export interface CommandTemplateCreateRequest {
  type: string;
  description: string;
  payloadSchema: PayloadFieldCreateDto[];
}

export interface CommandTemplateUpdateRequest {
  description: string;
  payloadSchema: PayloadFieldCreateDto[];
}

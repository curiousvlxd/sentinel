export interface SseEvent {
  eventId: string;
  missionId?: string | null;
  satelliteId?: string | null;
  type: string;
  ts: string;
  bucketStart?: string | null;
  payload?: unknown;
}

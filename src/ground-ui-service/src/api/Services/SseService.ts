import type { SseEvent } from "@/types/sse";
import { apiBase } from "@/api/client";

const getBaseUrl = () =>
  typeof window !== "undefined" ? apiBase : "";

function parseSseLine(line: string): SseEvent | null {
  if (!line.startsWith("data: ")) return null;
  try {
    const json = line.slice(6).trim();
    if (json === "[DONE]" || !json) return null;
    return JSON.parse(json) as SseEvent;
  } catch {
    return null;
  }
}

export function subscribeMissionEvents(
  missionId: string,
  onEvent: (evt: SseEvent) => void,
  onError?: (err: Error) => void
): () => void {
  const url = `${getBaseUrl()}/api/sse/missions/${missionId}`;
  const ac = new AbortController();

  const run = async () => {
    try {
      const res = await fetch(url, { signal: ac.signal });
      if (!res.ok || !res.body) {
        onError?.(new Error(`SSE ${res.status}`));
        return;
      }
      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";
      for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split("\n");
        buffer = lines.pop() ?? "";
        for (const line of lines) {
          const evt = parseSseLine(line);
          if (evt) onEvent(evt);
        }
      }
    } catch (err) {
      if ((err as Error).name !== "AbortError") {
        onError?.(err instanceof Error ? err : new Error(String(err)));
      }
    }
  };
  run();

  return () => ac.abort();
}

export function subscribeSatelliteEvents(
  satelliteId: string,
  onEvent: (evt: SseEvent) => void,
  onError?: (err: Error) => void
): () => void {
  const url = `${getBaseUrl()}/api/sse/satellites/${satelliteId}`;
  const ac = new AbortController();

  const run = async () => {
    try {
      const res = await fetch(url, { signal: ac.signal });
      if (!res.ok || !res.body) {
        onError?.(new Error(`SSE ${res.status}`));
        return;
      }
      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";
      for (;;) {
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split("\n");
        buffer = lines.pop() ?? "";
        for (const line of lines) {
          const evt = parseSseLine(line);
          if (evt) onEvent(evt);
        }
      }
    } catch (err) {
      if ((err as Error).name !== "AbortError") {
        onError?.(err instanceof Error ? err : new Error(String(err)));
      }
    }
  };
  run();

  return () => ac.abort();
}

import apiClient from "./client";

export function createResourceApi<
  T,
  TCreate = Partial<T>,
  TUpdate = Partial<T>,
>(
  base: string,
  extra?: Record<string, (...args: unknown[]) => unknown>
) {
  const api = {
    list: (params?: Record<string, string | number>) =>
      apiClient.get<T[]>(base, { params }).then((r) => r.data),
    get: (id: string) =>
      apiClient.get<T>(`${base}/${id}`).then((r) => r.data),
    create: (body: TCreate) =>
      apiClient.post<T>(base, body).then((r) => r.data),
    update: (id: string, body: TUpdate) =>
      apiClient.put<T>(`${base}/${id}`, body).then((r) => r.data),
    delete: (id: string) => apiClient.delete(`${base}/${id}`),
  };
  return { ...api, ...extra } as typeof api & Record<string, (...args: unknown[]) => unknown>;
}

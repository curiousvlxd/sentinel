import axios from "axios";

// Бэкенд вызывается напрямую (http://localhost:5276/api/), а не через хост UI.
const apiBase =
  (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5276").replace(/\/$/, "");
const apiBaseURL = `${apiBase}/api/`;

const apiClient = axios.create({
  baseURL: apiBaseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

export { apiBase };

let getTokenRef: (() => Promise<string | null>) | null = null;

export const setTokenFetcher = (fn: (() => Promise<string | null>) | null) => {
  getTokenRef = fn;
};

apiClient.interceptors.request.use(
  async (config) => {
    if (import.meta.env.DEV) {
      const url =
        (config.baseURL ?? "") +
        (config.url ?? "") +
        (config.params ? "?" + new URLSearchParams(config.params).toString() : "");
      console.log(`[API] ${(config.method ?? "get").toUpperCase()} ${url}`);
    }
    if (getTokenRef) {
      const token = await getTokenRef();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

apiClient.interceptors.response.use(
  (response) => {
    if (import.meta.env.DEV) {
      console.log(
        `[API] ${response.config.method?.toUpperCase()} ${response.config.url} → ${response.status}`
      );
    }
    return response;
  },
  (error) => {
    if (import.meta.env.DEV && error.config) {
      console.log(
        `[API] ${error.config.method?.toUpperCase()} ${error.config.url} → ${error.response?.status ?? error.code ?? "ERR"}`
      );
    }
    return Promise.reject(error);
  },
);

export default apiClient;

import {
  createContext,
  useCallback,
  useContext,
  useRef,
  useState,
  type ReactNode,
} from "react";

export type ToastVariant = "success" | "error" | "loading";

export interface ToastItem {
  id: string;
  message: string;
  variant: ToastVariant;
  durationMs: number;
  progress: number;
  createdAt: number;
}

interface ToastContextValue {
  toasts: ToastItem[];
  addToast: (message: string, options?: { variant?: ToastVariant; durationMs?: number }) => string;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const defaultDurationMs = 5000;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const timersRef = useRef<Map<string, { progressInterval: ReturnType<typeof setInterval>; endTimeout: ReturnType<typeof setTimeout> }>>(new Map());

  const removeToast = useCallback((id: string) => {
    const timers = timersRef.current.get(id);
    if (timers) {
      clearInterval(timers.progressInterval);
      clearTimeout(timers.endTimeout);
      timersRef.current.delete(id);
    }
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const addToast = useCallback(
    (message: string, options?: { variant?: ToastVariant; durationMs?: number }) => {
      const id = `toast-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
      const variant = options?.variant ?? "success";
      const durationMs = options?.durationMs ?? defaultDurationMs;
      const createdAt = Date.now();

      setToasts((prev) => [
        ...prev,
        { id, message, variant, durationMs, progress: 0, createdAt },
      ]);

      const tickMs = 50;
      const step = tickMs / durationMs;
      let progress = 0;
      const progressInterval = setInterval(() => {
        progress = Math.min(1, progress + step);
        setToasts((prev) =>
          prev.map((t) => (t.id === id ? { ...t, progress } : t))
        );
      }, tickMs);
      const endTimeout = setTimeout(() => {
        removeToast(id);
      }, durationMs);
      timersRef.current.set(id, { progressInterval, endTimeout });

      return id;
    },
    [removeToast]
  );

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within ToastProvider");
  return ctx;
}

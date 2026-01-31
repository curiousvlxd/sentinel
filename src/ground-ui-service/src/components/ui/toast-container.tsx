import { X } from "lucide-react";
import { useToast } from "@/lib/toast";

const variantStyles: Record<string, string> = {
  success: "border-emerald-500/40 bg-emerald-950/90 text-emerald-100",
  error: "border-red-500/40 bg-red-950/90 text-red-100",
  loading: "border-indigo-500/40 bg-indigo-950/90 text-indigo-100",
};

export function ToastContainer() {
  const { toasts, removeToast } = useToast();

  if (toasts.length === 0) return null;

  return (
    <div
      className="fixed bottom-4 right-4 z-[100] flex flex-col gap-3 max-w-sm w-full pointer-events-none"
      aria-live="polite"
    >
      <div className="flex flex-col gap-3 pointer-events-auto">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={`rounded-xl border shadow-lg overflow-hidden ${variantStyles[t.variant] ?? variantStyles.success}`}
            role="alert"
          >
            <div className="flex items-start gap-3 p-4 pr-10">
              <p className="text-sm font-medium flex-1">{t.message}</p>
              <button
                type="button"
                onClick={() => removeToast(t.id)}
                className="absolute top-3 right-3 p-1 rounded-md opacity-70 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-white/30"
                aria-label="Close"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
            <div className="h-1 bg-black/20">
              <div
                className="h-full bg-white/30 transition-[width] duration-75 ease-linear"
                style={{ width: `${t.progress * 100}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

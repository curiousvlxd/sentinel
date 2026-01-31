import { CommandsApiService } from "@/api/Services/CommandsApiService";
import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import {
  subscribeSatelliteEvents,
} from "@/api/Services/SseService";
import apiClient from "@/api/client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import type {
  CommandDto,
  CommandTemplateDto,
  PayloadFieldCreateDto,
} from "@/types/command";
import type { MlResultDto } from "@/types/mlResult";
import type { SatelliteDto } from "@/types/satelliteApi";
import type { SimScenario } from "@/types/sim";
import type { SseEvent } from "@/types/sse";
import {
  Activity,
  Braces,
  Cpu,
  Heart,
  ListOrdered,
  Pencil,
  Plus,
  Send,
  Trash2,
  Zap,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { formatUtcInUserTz } from "@/lib/dateUtils";
import { useToast } from "@/lib/toast";
import CardWrap from "../Components/CardWrap";

const stateClass = (state: string) =>
  state === "Ok"
    ? "bg-emerald-500/20 text-emerald-300 border-emerald-500/30"
    : state === "Risk"
      ? "bg-amber-500/20 text-amber-300 border-amber-500/30"
      : "bg-red-500/20 text-red-300 border-red-500/30";

const modeClass = (mode: string) =>
  mode === "Autonomous"
    ? "bg-violet-500/20 text-violet-300 border-violet-500/30"
    : mode === "Assisted"
      ? "bg-sky-500/20 text-sky-300 border-sky-500/30"
      : "bg-slate-500/20 text-slate-300 border-slate-500/30";

const linkClass = (link: string) =>
  link === "Online"
    ? "bg-emerald-500/20 text-emerald-300 border-emerald-500/30"
    : "bg-slate-500/20 text-slate-700 dark:text-slate-400 border-slate-500/30";

const eventsCacheBySatellite: Record<string, SseEvent[]> = {};
const SESSION_STORAGE_KEY = "satellite-events";

function loadEventsFromSession(satelliteId: string): SseEvent[] {
  try {
    const raw = sessionStorage.getItem(`${SESSION_STORAGE_KEY}-${satelliteId}`);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as SseEvent[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function saveEventsToSession(satelliteId: string, list: SseEvent[]) {
  try {
    sessionStorage.setItem(`${SESSION_STORAGE_KEY}-${satelliteId}`, JSON.stringify(list));
  } catch {
  }
}

const HEALTHCHECK_TYPES = ["healthcheck", "ml_result", "decision", "link.window.open", "link.window.close", "satellite.state.changed"] as const;

function isHealthcheckEvent(e: SseEvent): boolean {
  return HEALTHCHECK_TYPES.includes(e.type as (typeof HEALTHCHECK_TYPES)[number]) || e.type.startsWith("command.");
}

interface HealthcheckPayload {
  mlResult?: Record<string, unknown>;
  decision?: { type?: string; reason?: string };
}

function normalizeMlResult(raw: MlResultDto | undefined): MlResultDto | undefined {
  if (!raw) return undefined;
  const perSignalScore =
    typeof raw.perSignalScore === "object" && raw.perSignalScore !== null
      ? raw.perSignalScore
      : typeof raw.perSignalScore === "string"
        ? (() => {
            try {
              return JSON.parse(raw.perSignalScore) as Record<string, number>;
            } catch {
              return {};
            }
          })()
        : {};
  const topContributors = Array.isArray(raw.topContributors)
    ? raw.topContributors
    : typeof raw.topContributors === "string"
      ? (() => {
          try {
            return JSON.parse(raw.topContributors) as Array<{ key: string; weight: number }>;
          } catch {
            return [];
          }
        })()
      : [];
  return { ...raw, perSignalScore, topContributors };
}

const SatelliteDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [satellite, setSatellite] = useState<SatelliteDto | null>(null);
  const [mlResults, setMlResults] = useState<MlResultDto[]>([]);
  const [commands, setCommands] = useState<CommandDto[]>([]);
  const [events, setEvents] = useState<SseEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [templates, setTemplates] = useState<CommandTemplateDto[]>([]);
  const [cmdType, setCmdType] = useState("");
  const [cmdPriority, setCmdPriority] = useState(5);
  const [cmdTtl, setCmdTtl] = useState(300);
  const [payloadValues, setPayloadValues] = useState<Record<string, string | number>>({});
  const [simLoading, setSimLoading] = useState(false);
  const [simError, setSimError] = useState<string | null>(null);
  const [simScenario, setSimScenario] = useState<SimScenario>("Normal");
  const toast = useToast();
  const [templateFormOpen, setTemplateFormOpen] = useState(false);
  const [editingTemplateId, setEditingTemplateId] = useState<string | null>(null);
  const [formType, setFormType] = useState("");
  const [formDescription, setFormDescription] = useState("");
  const [formPayloadSchema, setFormPayloadSchema] = useState<PayloadFieldCreateDto[]>([]);
  const [templateSaving, setTemplateSaving] = useState(false);
  const [healthcheckExpandedJsonId, setHealthcheckExpandedJsonId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<"activity" | "commands" | "simulator">("activity");

  const formatPayloadValue = (v: unknown): string => {
    if (v == null) return "—";
    if (typeof v === "number") return Number.isInteger(v) ? String(v) : v.toFixed(4);
    if (typeof v === "boolean") return v ? "true" : "false";
    if (typeof v === "object") {
      const s = JSON.stringify(v);
      return s.length > 80 ? s.slice(0, 80) + "…" : s;
    }
    const s = String(v);
    return s.length > 80 ? s.slice(0, 80) + "…" : s;
  };

  const parsePayloadValue = (v: unknown): unknown => {
    if (v == null) return v;
    if (typeof v === "string" && (v.trim().startsWith("{") || v.trim().startsWith("["))) {
      try {
        return JSON.parse(v) as unknown;
      } catch {
        return v;
      }
    }
    return v;
  };

  const isMlResultPayload = (type: string, p: unknown): boolean => {
    if (type === "ml_result") return true;
    if (p == null || typeof p !== "object") return false;
    const o = p as Record<string, unknown>;
    return (
      o["anomaly score"] != null ||
      o.anomaly_score != null ||
      o.anomalyScore != null
    );
  };

  const getMlPayloadField = (payload: Record<string, unknown>, ...keys: string[]): unknown => {
    for (const k of keys) {
      if (payload[k] !== undefined && payload[k] !== null) return payload[k];
    }
    return undefined;
  };

  const MlResultPayloadView = ({ payload }: { payload: Record<string, unknown> }) => {
    const anomalyScoreRaw = getMlPayloadField(payload, "anomaly score", "anomaly_score", "anomalyScore");
    const anomalyScore = typeof anomalyScoreRaw === "number" ? anomalyScoreRaw : undefined;
    const confidenceRaw = getMlPayloadField(payload, "confidence");
    const confidence = typeof confidenceRaw === "number" ? confidenceRaw : undefined;
    const perSignalRaw = parsePayloadValue(
      getMlPayloadField(payload, "per signal score", "per_signal_score", "perSignalScore")
    );
    const perSignal =
      perSignalRaw != null && typeof perSignalRaw === "object" && !Array.isArray(perSignalRaw)
        ? (perSignalRaw as Record<string, number>)
        : undefined;
    const topContributorsRaw = parsePayloadValue(
      getMlPayloadField(payload, "top contributors", "top_contributors", "topContributors")
    );
    const topContributors = Array.isArray(topContributorsRaw)
      ? (topContributorsRaw as Array<{ key: string; weight: number }>)
      : undefined;
    const modelRaw = parsePayloadValue(getMlPayloadField(payload, "model"));
    const modelObj =
      modelRaw != null && typeof modelRaw === "object" && !Array.isArray(modelRaw)
        ? (modelRaw as Record<string, unknown>)
        : undefined;
    const modelName = modelObj?.name != null ? String(modelObj.name) : undefined;
    const modelVersion = modelObj?.version != null ? String(modelObj.version) : undefined;

    return (
      <div className="mt-3 space-y-3">
        {(modelName != null || modelVersion != null) && (
          <div>
            <span className="text-[10px] font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">
              Model
            </span>
            <div className="mt-1 text-xs text-slate-800 dark:text-slate-200">
              {modelName ?? "—"} <span className="text-slate-500 dark:text-slate-400">/</span> {modelVersion ?? "—"}
            </div>
          </div>
        )}
        <div className="flex flex-wrap gap-3">
          {anomalyScore != null && (
            <div className="rounded-lg bg-slate-100 dark:bg-[#1e2a3a] px-3 py-2">
              <span className="text-[10px] font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">
                anomaly score
              </span>
              <div className="mt-0.5 text-sm font-mono font-semibold text-slate-800 dark:text-indigo-200">
                {Number(anomalyScore).toFixed(4)}
              </div>
            </div>
          )}
          {confidence != null && (
            <div className="rounded-lg bg-slate-100 dark:bg-[#1e2a3a] px-3 py-2">
              <span className="text-[10px] font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">
                confidence
              </span>
              <div className="mt-0.5 text-sm font-mono font-semibold text-slate-800 dark:text-indigo-200">
                {(Number(confidence) * 100).toFixed(2)}%
              </div>
            </div>
          )}
        </div>
        {perSignal != null && Object.keys(perSignal).length > 0 && (
          <div>
            <span className="text-[10px] font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">
              per signal score
            </span>
            <div className="mt-1.5 flex flex-wrap gap-2">
              {Object.entries(perSignal).map(([k, v]) => (
                <span
                  key={k}
                  className="rounded-md border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244] px-2 py-1 text-xs font-mono text-slate-800 dark:text-slate-200"
                >
                  {k.replace(/_/g, " ")}: {(Number(v) * 100).toFixed(1)}%
                </span>
              ))}
            </div>
          </div>
        )}
        {topContributors != null && topContributors.length > 0 && (
          <div>
            <span className="text-[10px] font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">
              top contributors
            </span>
            <ul className="mt-1.5 space-y-0.5 text-xs text-slate-700 dark:text-slate-300">
              {topContributors.map((c, i) => (
                <li key={i}>
                  <span className="font-medium text-slate-800 dark:text-slate-200">{c.key}</span>
                  {" "}
                  <span className="font-mono text-slate-600 dark:text-slate-400">
                    ({(Number(c.weight) * 100).toFixed(1)}%)
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    );
  };

  const loadSatellite = useCallback(async () => {
    if (!id) return;
    const s = await SatellitesApiService.get(id);
    setSatellite(s);
  }, [id]);

  const loadMlResults = useCallback(async () => {
    if (!id) return;
    const from = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
    const to = new Date().toISOString();
    const data = await SatellitesApiService.getMlResults(id, from, to);
    setMlResults(data.response);
  }, [id]);

  const loadCommands = useCallback(async () => {
    if (!id) return;
    const list = await CommandsApiService.listBySatellite(id);
    setCommands(list);
  }, [id]);

  const loadTemplates = useCallback(async () => {
    const list = await CommandsApiService.getTemplates();
    setTemplates(list);
  }, []);

  useEffect(() => {
    if (!id) return;
    const list = Array.isArray(eventsCacheBySatellite[id]) ? eventsCacheBySatellite[id] : loadEventsFromSession(id);
    setEvents(Array.isArray(list) ? list : []);
  }, [id]);

  useEffect(() => {
    if (!id) return;
    const run = async () => {
      setLoading(true);
      try {
        await loadSatellite();
        await loadMlResults();
        await loadCommands();
        await loadTemplates();
      } finally {
        setLoading(false);
      }
    };
    run();
  }, [id, loadSatellite, loadMlResults, loadCommands, loadTemplates]);

  useEffect(() => {
    if (!id) return;
    const subscriptionSatelliteId = id;
    const unsubscribe = subscribeSatelliteEvents(id, (evt) => {
      setEvents((prev) => {
        const merged = [evt, ...prev.filter((e) => String(e.eventId) !== String(evt.eventId))];
        const next = merged.slice(0, 50);
        eventsCacheBySatellite[subscriptionSatelliteId] = next;
        saveEventsToSession(subscriptionSatelliteId, next);
        return next;
      });
    });
    return () => unsubscribe();
  }, [id]);

  const selectedTemplate = templates.find((t) => t.type === cmdType);
  const payloadSchema = selectedTemplate?.payloadSchema ?? [];

  const handleCreateCommand = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !cmdType.trim()) return;
    const payloadJson =
      payloadSchema.length > 0
        ? JSON.stringify(payloadValues)
        : undefined;
    try {
      await CommandsApiService.create(id, {
        type: cmdType.trim(),
        commandTemplateId: selectedTemplate?.id,
        priority: cmdPriority,
        ttlSec: cmdTtl,
        payloadJson: payloadJson || undefined,
      });
      await loadCommands();
      setCmdType("");
      setPayloadValues({});
      toast.addToast("Command created");
    } catch (err) {
      const msg = err && typeof err === "object" && "message" in err ? String((err as Error).message) : "Failed to create command";
      toast.addToast(msg, { variant: "error" });
    }
  };

  const handleTypeChange = (type: string) => {
    setCmdType(type);
    const t = templates.find((x) => x.type === type);
    const next: Record<string, string | number> = {};
    if (t?.payloadSchema) {
      for (const f of t.payloadSchema) {
        next[f.name] =
          f.default != null
            ? typeof f.default === "number"
              ? f.default
              : String(f.default)
            : f.fieldType === "number"
              ? 0
              : "";
      }
    }
    setPayloadValues(next);
  };

  const handleExecuteCommand = async (commandId: string) => {
    try {
      await CommandsApiService.execute(commandId);
      await loadCommands();
      toast.addToast("Command executed");
    } catch (err) {
      const msg = err && typeof err === "object" && "message" in err ? String((err as Error).message) : "Failed to execute command";
      toast.addToast(msg, { variant: "error" });
    }
  };

  const openAddTemplate = () => {
    setEditingTemplateId(null);
    setFormType("");
    setFormDescription("");
    setFormPayloadSchema([]);
    setTemplateFormOpen(true);
  };

  const openEditTemplate = (t: CommandTemplateDto) => {
    setEditingTemplateId(t.id);
    setFormType(t.type);
    setFormDescription(t.description);
    setFormPayloadSchema(
      (t.payloadSchema ?? []).map((f) => ({
        name: f.name,
        fieldType: f.fieldType ?? "number",
        unit: typeof f.unit === "number" ? f.unit : 0,
        defaultValue: f.default != null ? String(f.default) : "",
      }))
    );
    setTemplateFormOpen(true);
  };

  const cancelTemplateForm = () => {
    setTemplateFormOpen(false);
    setEditingTemplateId(null);
  };

  const addFormField = () => {
    setFormPayloadSchema((prev) => [
      ...prev,
      { name: "", fieldType: "number", unit: 0, defaultValue: "" },
    ]);
  };

  const updateFormField = (index: number, patch: Partial<PayloadFieldCreateDto>) => {
    setFormPayloadSchema((prev) =>
      prev.map((f, i) => (i === index ? { ...f, ...patch } : f))
    );
  };

  const removeFormField = (index: number) => {
    setFormPayloadSchema((prev) => prev.filter((_, i) => i !== index));
  };

  const saveTemplate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formType.trim()) return;
    setTemplateSaving(true);
    try {
      const payload = formPayloadSchema.map((f) => ({
        name: f.name.trim() || "field",
        fieldType: f.fieldType.trim() || "number",
        unit: f.unit,
        defaultValue: f.defaultValue?.trim() || null,
      }));
      if (editingTemplateId) {
        await CommandsApiService.updateTemplate(editingTemplateId, {
          description: formDescription.trim(),
          payloadSchema: payload,
        });
        toast.addToast("Template updated");
      } else {
        await CommandsApiService.createTemplate({
          type: formType.trim(),
          description: formDescription.trim(),
          payloadSchema: payload,
        });
        toast.addToast("Template created");
      }
      await loadTemplates();
      cancelTemplateForm();
    } catch (err) {
      const msg = err && typeof err === "object" && "message" in err ? String((err as Error).message) : "Failed to save template";
      toast.addToast(msg, { variant: "error" });
    } finally {
      setTemplateSaving(false);
    }
  };

  const deleteTemplate = async (templateId: string) => {
    try {
      await CommandsApiService.deleteTemplate(templateId);
      toast.addToast("Template deleted");
      await loadTemplates();
      if (editingTemplateId === templateId) cancelTemplateForm();
    } catch (err) {
      const msg = err && typeof err === "object" && "message" in err ? String((err as Error).message) : "Failed to delete template";
      toast.addToast(msg, { variant: "error" });
    }
  };

  const handleSimStart = async () => {
    if (!id) return;
    setSimLoading(true);
    setSimError(null);
    try {
      await apiClient.post(`satellites/${id}/sim/start`, {
        scenario: simScenario,
      });
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "response" in err
          ? String(
              (err as { response?: { data?: unknown } }).response?.data ??
                (err as unknown as Error).message
            )
          : String(err);
      setSimError(msg);
    } finally {
      setSimLoading(false);
    }
  };

  const handleSimStop = async () => {
    if (!id) return;
    setSimLoading(true);
    setSimError(null);
    try {
      await apiClient.post(`satellites/${id}/sim/stop`);
    } catch (err: unknown) {
      const msg =
        err && typeof err === "object" && "response" in err
          ? String(
              (err as { response?: { data?: unknown } }).response?.data ??
                (err as unknown as Error).message
            )
          : String(err);
      setSimError(msg);
    } finally {
      setSimLoading(false);
    }
  };

  const latestMl = normalizeMlResult(mlResults[0]);
  const canSendCommands = satellite?.mode !== "Autonomous";
  const healthcheckEvents = events.filter(isHealthcheckEvent);
  const telemetryEvents = events.filter((e) => e.type === "telemetry");

  if (loading && !satellite) {
    return (
      <CardWrap>
        <Skeleton className="h-12 w-64 mb-4 bg-slate-300 dark:bg-[#626a76]/40" />
        <Skeleton className="h-24 w-full mb-4 bg-slate-300 dark:bg-[#626a76]/40" />
      </CardWrap>
    );
  }

  if (!satellite) {
    return (
      <CardWrap>
        <p className="text-slate-700 dark:text-slate-400">Satellite not found.</p>
        <Button onClick={() => navigate("/satellites")} className="mt-4">
          Back to satellites
        </Button>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <div className="mb-8 flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0 flex-1">
          <h1 className="text-2xl font-semibold text-slate-900 dark:text-indigo-100 tracking-tight">
            {satellite.name}
          </h1>
          <div className="mt-3 flex flex-wrap items-center gap-2">
            <span
              className={`inline-flex items-center rounded-md border px-2.5 py-1 text-xs font-medium ${stateClass(
                satellite.state
              )}`}
            >
              {satellite.state}
            </span>
            <span
              className={`inline-flex items-center rounded-md border px-2.5 py-1 text-xs font-medium ${modeClass(
                satellite.mode
              )}`}
            >
              {satellite.mode}
            </span>
            <span
              className={`inline-flex items-center rounded-md border px-2.5 py-1 text-xs font-medium ${linkClass(
                satellite.linkStatus
              )}`}
            >
              {satellite.linkStatus}
            </span>
            {satellite.lastBucketStart && (
              <span className="text-xs text-slate-700 dark:text-slate-500">
                Last bucket: {formatUtcInUserTz(satellite.lastBucketStart)}
              </span>
            )}
          </div>
          {satellite.missionName && (
            <p className="mt-2 text-sm text-slate-700 dark:text-slate-500">
              Mission:{" "}
              <button
                type="button"
                onClick={() =>
                  satellite.missionId && navigate(`/missions/${satellite.missionId}`)
                }
                className="text-indigo-400 hover:underline"
              >
                {satellite.missionName}
              </button>
            </p>
          )}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigate(`/satellites/${id}/edit`)}
          className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300 shrink-0"
        >
          Edit
        </Button>
      </div>

      <div className="mb-4 flex flex-wrap rounded-xl border border-slate-200 dark:border-slate-600 overflow-hidden bg-slate-50 dark:bg-[#1e2a3a] p-1 gap-1">
        {(
          [
            { id: "activity" as const, label: "Activity", icon: Activity },
            { id: "commands" as const, label: "Commands", icon: Send },
            { id: "simulator" as const, label: "Simulator", icon: Zap },
          ] as const
        ).map(({ id: tabId, label, icon: Icon }) => (
          <button
            key={tabId}
            type="button"
            data-tab={tabId}
            onClick={() => setActiveTab(tabId)}
            className={`min-h-9 px-3 py-2 text-sm font-medium rounded-lg transition-colors flex items-center gap-1.5 shrink-0 ${
              activeTab === tabId
                ? "bg-indigo-600 text-white shadow-sm"
                : "text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700/50"
            }`}
          >
            <Icon className="h-4 w-4 shrink-0" />
            {label}
          </button>
        ))}
      </div>

      {activeTab === "activity" && (
      <>
      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Activity className="h-5 w-5 shrink-0" />
          Telemetry (last 1m bucket)
        </h2>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
          {[
            "cpu_temperature",
            "battery_voltage",
            "power_consumption",
            "signal_strength",
            "pressure",
            "gyro_speed",
          ].map((key) => (
            <div
              key={key}
              className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-4 text-center min-h-[72px] flex flex-col justify-center"
            >
              <div className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide">
                {key.replace(/_/g, " ")}
              </div>
              <div className="mt-1.5 text-sm font-mono text-slate-800 dark:text-slate-200">
                {latestMl?.perSignalScore?.[key] != null
                  ? `${(latestMl.perSignalScore[key] * 100).toFixed(1)}%`
                  : "—"}
              </div>
            </div>
          ))}
        </div>
        {satellite.lastBucketStart && (
          <p className="mt-3 text-xs text-slate-700 dark:text-slate-500">
            Bucket start: {formatUtcInUserTz(satellite.lastBucketStart)}
          </p>
        )}
      </section>

      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Cpu className="h-5 w-5 shrink-0" />
          ML panel
        </h2>
        <div className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-5 min-h-[140px]">
          {latestMl ? (
            <div className="space-y-4">
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                <div>
                  <div className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide mb-0.5">
                    anomaly_score
                  </div>
                  <div className="text-lg font-mono text-indigo-300">
                    {latestMl.anomalyScore.toFixed(4)}
                  </div>
                </div>
                <div>
                  <div className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide mb-0.5">
                    confidence
                  </div>
                  <div className="text-lg font-mono text-indigo-300">
                    {(latestMl.confidence * 100).toFixed(2)}%
                  </div>
                </div>
              </div>
              {latestMl.perSignalScore && Object.keys(latestMl.perSignalScore).length > 0 && (
                <div>
                  <div className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide mb-2">
                    per_signal_score
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {Object.entries(latestMl.perSignalScore).map(([k, v]) => (
                      <span
                        key={k}
                        className="rounded-md border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244] px-2.5 py-1 text-xs text-slate-800 dark:text-slate-200"
                      >
                        {k}: {(v * 100).toFixed(0)}%
                      </span>
                    ))}
                  </div>
                </div>
              )}
              {latestMl.topContributors && latestMl.topContributors.length > 0 && (
                <div>
                  <div className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide mb-1.5">
                    top_contributors
                  </div>
                  <ul className="space-y-0.5 text-sm text-slate-700 dark:text-slate-300">
                    {latestMl.topContributors.map((c, i) => (
                      <li key={i}>
                        {c.key}: {(c.weight * 100).toFixed(1)}%
                      </li>
                    ))}
                  </ul>
                </div>
              )}
              <p className="text-xs text-slate-600 pt-1 border-t border-slate-200 dark:border-slate-600/50 dark:text-slate-500">
                Model: {latestMl.modelName ?? "—"} / {latestMl.modelVersion ?? "—"}
              </p>
            </div>
          ) : (
            <p className="text-slate-600 dark:text-slate-500 py-2">No ML result yet.</p>
          )}
        </div>
      </section>

      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Heart className="h-5 w-5 shrink-0" />
          Healthcheck feed
        </h2>
        <div className="min-h-[280px] max-h-[60vh] overflow-y-auto rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-4 space-y-3">
          {healthcheckEvents.length === 0 ? (
            <p className="text-sm text-slate-600 dark:text-slate-500 py-4">No healthcheck events yet.</p>
          ) : (
            healthcheckEvents.map((evt) => {
              const payload = evt.payload as HealthcheckPayload | Record<string, unknown> | null;
              const isUnified = evt.type === "healthcheck" && payload != null && typeof payload === "object" && ("mlResult" in payload || "decision" in payload);
              const mlResultPayload = isUnified ? (payload as HealthcheckPayload).mlResult : undefined;
              const decisionPayload = isUnified ? (payload as HealthcheckPayload).decision : undefined;
              return (
                <div
                  key={evt.eventId}
                  className="rounded-lg border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244] px-4 py-3 text-sm"
                >
                  <div className="flex items-center gap-2 flex-wrap mb-2">
                    <span className="font-semibold text-slate-800 dark:text-indigo-200">{evt.type}</span>
                    <span className="text-xs text-slate-700 dark:text-slate-500">{formatUtcInUserTz(evt.ts)}</span>
                  </div>
                  {evt.payload != null && (
                    <>
                      {healthcheckExpandedJsonId === evt.eventId ? (
                        <pre className="mt-2 rounded-lg bg-slate-100 dark:bg-[#1e2a3a] p-3 text-xs text-slate-700 dark:text-slate-400 overflow-x-auto max-h-48">
                          {JSON.stringify(evt.payload, null, 2)}
                        </pre>
                      ) : isUnified && (mlResultPayload != null || decisionPayload != null) ? (
                        <div className="mt-2 space-y-4">
                          {mlResultPayload != null && (
                            <div className="pl-3 border-l-2 border-indigo-500/40">
                              <div className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-1.5">ML result</div>
                              <MlResultPayloadView payload={mlResultPayload as Record<string, unknown>} />
                            </div>
                          )}
                          {decisionPayload != null && (
                            <div className="pl-3 border-l-2 border-emerald-500/40">
                              <div className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wide mb-1.5">Decision</div>
                              <div className="flex flex-wrap items-center gap-2">
                                {decisionPayload.type != null && (
                                  <span className={`inline-flex rounded-md border px-2 py-0.5 text-xs font-medium ${stateClass(decisionPayload.type)}`}>
                                    {decisionPayload.type}
                                  </span>
                                )}
                                {decisionPayload.reason != null && (
                                  <span className="text-xs text-slate-700 dark:text-slate-300">{decisionPayload.reason}</span>
                                )}
                              </div>
                            </div>
                          )}
                        </div>
                      ) : isMlResultPayload(evt.type, evt.payload) ? (
                        <MlResultPayloadView payload={evt.payload as Record<string, unknown>} />
                      ) : (
                        <div className="mt-2 space-y-1.5">
                          {Object.entries(evt.payload as Record<string, unknown>).map(([key, val]) => (
                            <div key={key} className="flex flex-wrap items-baseline gap-x-2 text-xs">
                              <span className="font-medium text-slate-600 dark:text-slate-400 shrink-0">
                                {key.replace(/_/g, " ")}:
                              </span>
                              <span className="text-slate-800 dark:text-slate-200 font-mono break-all">
                                {formatPayloadValue(val)}
                              </span>
                            </div>
                          ))}
                        </div>
                      )}
                      <button
                        type="button"
                        onClick={() =>
                          setHealthcheckExpandedJsonId((prev) =>
                            prev === evt.eventId ? null : evt.eventId
                          )
                        }
                        className="mt-2 inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[10px] font-medium text-slate-500 dark:text-slate-500 hover:text-slate-700 dark:hover:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700/50 transition-colors"
                        title={healthcheckExpandedJsonId === evt.eventId ? "Hide JSON" : "Show as JSON"}
                      >
                        <Braces className="h-2.5 w-2.5 shrink-0" />
                        {healthcheckExpandedJsonId === evt.eventId ? "hide" : "json"}
                      </button>
                    </>
                  )}
                </div>
              );
            })
          )}
        </div>
      </section>

      <section>
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Activity className="h-5 w-5 shrink-0" />
          Telemetry feed
        </h2>
        <div className="min-h-[320px] max-h-[60vh] overflow-y-auto rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-4 space-y-3">
          {telemetryEvents.length === 0 ? (
            <p className="text-sm text-slate-600 dark:text-slate-500 py-4">No telemetry yet. Start the simulator.</p>
          ) : (
            telemetryEvents.map((evt) => {
              const p = evt.payload as { timestamp?: string; location?: { lat?: number; lon?: number; altKm?: number }; signals?: Record<string, number> } | null;
              return (
                <div
                  key={evt.eventId}
                  className="rounded-lg border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244] px-4 py-3 text-sm"
                >
                  <div className="flex items-center gap-2 flex-wrap mb-2">
                    <span className="font-medium text-indigo-300">telemetry</span>
                    <span className="text-xs text-slate-700 dark:text-slate-500">{formatUtcInUserTz(p?.timestamp ?? evt.ts)}</span>
                  </div>
                  {p?.location && (
                    <div className="text-xs text-slate-700 dark:text-slate-400 mb-1.5">
                      lat {p.location.lat?.toFixed(4)} · lon {p.location.lon?.toFixed(4)} · alt {p.location.altKm?.toFixed(0)} km
                    </div>
                  )}
                  {p?.signals && (
                    <div className="flex flex-wrap gap-x-3 gap-y-1 text-xs text-slate-700 dark:text-slate-300">
                      {Object.entries(p.signals).map(([k, v]) => (
                        <span key={k}>{k.replace(/([A-Z])/g, " $1").trim()}: {typeof v === "number" ? v.toFixed(2) : String(v)}</span>
                      ))}
                    </div>
                  )}
                </div>
              );
            })
          )}
        </div>
      </section>
      </>
      )}

      {activeTab === "commands" && (
      <>
      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Send className="h-5 w-5 shrink-0" />
          Commands
        </h2>
        {!canSendCommands && (
          <div className="mb-4 rounded-xl border border-amber-500/30 bg-amber-500/10 p-4 text-sm text-amber-200/90">
            Commands are only available for satellites in <strong>Assisted</strong> or <strong>Manual</strong> mode. This satellite is in Autonomous mode.
          </div>
        )}
        <div
          className={`rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-5 ${!canSendCommands ? "opacity-60 pointer-events-none" : ""}`}
        >
          <form
            onSubmit={handleCreateCommand}
            className="mb-5 flex flex-wrap gap-4 items-end"
          >
            <div>
              <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">
                Type
              </label>
              <select
                value={cmdType}
                onChange={(e) => handleTypeChange(e.target.value)}
                disabled={!canSendCommands}
                className="h-9 min-w-[160px] rounded-lg border border-slate-200 dark:border-slate-600 bg-white dark:bg-[#263244] px-3 text-sm text-slate-900 dark:text-indigo-100 focus:border-indigo-500 focus:outline-none disabled:opacity-50"
              >
                <option value="">Select...</option>
                {templates.map((t) => (
                  <option key={t.id} value={t.type}>
                    {t.type}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">
                Priority
              </label>
              <Input
                type="number"
                value={cmdPriority}
                onChange={(e) => setCmdPriority(Number(e.target.value))}
                disabled={!canSendCommands}
                className="bg-white dark:bg-[#263244] border-slate-200 dark:border-slate-600 w-20 h-9"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">
                TTL (sec)
              </label>
              <Input
                type="number"
                value={cmdTtl}
                onChange={(e) => setCmdTtl(Number(e.target.value))}
                disabled={!canSendCommands}
                className="bg-white dark:bg-[#263244] border-slate-200 dark:border-slate-600 w-24 h-9"
              />
            </div>
            {payloadSchema.map((f) => (
              <div key={f.name}>
                <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">
                  {f.name}
                  {f.unit ? ` (${f.unit})` : ""}
                </label>
                <Input
                  type={f.fieldType === "number" ? "number" : "text"}
                  value={payloadValues[f.name] ?? (f.default != null ? String(f.default) : "")}
                  onChange={(e) =>
                    setPayloadValues((prev) => ({
                      ...prev,
                      [f.name]:
                        f.fieldType === "number"
                          ? Number(e.target.value)
                          : e.target.value,
                    }))
                  }
                  disabled={!canSendCommands}
                  className="bg-white dark:bg-[#263244] border-slate-200 dark:border-slate-600 w-32 h-9"
                />
              </div>
            ))}
            <Button
              type="submit"
              disabled={!cmdType.trim() || !canSendCommands}
              className="bg-indigo-600 hover:bg-indigo-500 text-white h-9 shrink-0"
            >
              Create command
            </Button>
          </form>
<div className="overflow-x-auto rounded-lg border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244]/50">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-400">
                  <th className="py-3.5 px-5 font-medium">Type</th>
                  <th className="py-3.5 px-5 font-medium">Status</th>
                  <th className="py-3.5 px-5 font-medium">Priority</th>
                  <th className="py-3.5 px-5 font-medium">TTL</th>
                  <th className="py-3.5 px-5 font-medium">Created</th>
                  <th className="py-3.5 px-5 font-medium"></th>
                </tr>
              </thead>
              <tbody>
                {commands.slice(0, 20).map((c) => (
                  <tr
                    key={c.id}
                    className="border-b border-slate-200 dark:border-slate-700/50 text-slate-800 dark:text-slate-300 last:border-0"
                  >
                    <td className="py-3.5 px-5">{c.type}</td>
                    <td className="py-3.5 px-5">{c.status}</td>
                    <td className="py-3.5 px-5">{c.priority}</td>
                    <td className="py-3.5 px-5">{c.ttlSec}s</td>
                    <td className="py-3.5 px-5">
                      {formatUtcInUserTz(c.createdAt)}
                    </td>
                    <td className="py-3.5 px-5">
                      {(c.status === "Queued" || c.status === "Claimed") && canSendCommands && (
                        <Button
                          type="button"
                          size="sm"
                          variant="outline"
                          className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300"
                          onClick={() => handleExecuteCommand(c.id)}
                        >
                          Execute
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {commands.length === 0 && (
              <p className="p-5 text-slate-600 dark:text-slate-500 text-sm">No commands.</p>
            )}
          </div>
        </div>
      </section>

      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <ListOrdered className="h-5 w-5 shrink-0" />
          Command templates
        </h2>
        <div className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-5">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
            <p className="text-sm text-slate-700 dark:text-slate-400">Manage command types and payload schema.</p>
            <Button
              type="button"
              size="sm"
              onClick={openAddTemplate}
              className="bg-indigo-600 hover:bg-indigo-500 text-white shrink-0 inline-flex items-center gap-2"
            >
              <Plus className="h-4 w-4" />
              Add template
            </Button>
          </div>
          <div className="overflow-x-auto rounded-lg border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244]/50 mb-6">
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-400">
                  <th className="pb-3 pt-3 pl-4 pr-4 font-medium">Type</th>
                  <th className="pb-3 pt-3 pr-4 font-medium">Description</th>
                  <th className="pb-3 pt-3 pr-4 font-medium w-24"></th>
                </tr>
              </thead>
              <tbody>
                {templates.map((t) => (
                  <tr key={t.id} className="border-b border-slate-200 dark:border-slate-700/50 text-slate-800 dark:text-slate-300 last:border-0">
                    <td className="py-3 pl-4 pr-4 font-medium">{t.type}</td>
                    <td className="py-3 pr-4">{t.description || "—"}</td>
                    <td className="py-3.5 px-5 flex gap-2">
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300 h-8 px-2"
                        onClick={() => openEditTemplate(t)}
                        aria-label="Edit template"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        className="border-red-600/50 text-red-300 h-8 px-2 hover:bg-red-500/10"
                        onClick={() => deleteTemplate(t.id)}
                        aria-label="Delete template"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {templates.length === 0 && !templateFormOpen && (
              <p className="p-5 text-slate-600 dark:text-slate-500 text-sm">No templates. Add one to define command types.</p>
            )}
          </div>
          {templateFormOpen && (
            <form onSubmit={saveTemplate} className="rounded-lg border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244]/50 p-5 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">Type</label>
                  <Input
                    value={formType}
                    onChange={(e) => setFormType(e.target.value)}
                    disabled={!!editingTemplateId}
                    className="bg-slate-100 dark:bg-[#1e2a3a] border-slate-200 dark:border-slate-600"
                    placeholder="e.g. ReducePower"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-1.5 uppercase tracking-wide">Description</label>
                  <Input
                    value={formDescription}
                    onChange={(e) => setFormDescription(e.target.value)}
                    className="bg-slate-100 dark:bg-[#1e2a3a] border-slate-200 dark:border-slate-600"
                    placeholder="Short description"
                  />
                </div>
              </div>
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-xs font-medium text-slate-700 dark:text-slate-400 uppercase tracking-wide">Payload schema</label>
                  <Button type="button" size="sm" variant="outline" className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300" onClick={addFormField}>
                    <Plus className="h-3.5 w-3.5 mr-1" />
                    Add field
                  </Button>
                </div>
                <div className="space-y-2 max-h-48 overflow-y-auto">
                  {formPayloadSchema.map((f, i) => (
                    <div key={i} className="flex flex-wrap gap-2 items-center rounded border border-slate-200 dark:border-slate-600/50 p-2 bg-slate-100 dark:bg-[#1e2a3a]/50">
                      <Input
                        value={f.name}
                        onChange={(e) => updateFormField(i, { name: e.target.value })}
                        placeholder="name"
                        className="bg-white dark:bg-[#263244] border-slate-200 dark:border-slate-600 w-32 flex-1 min-w-[80px] h-8 text-sm"
                      />
                      <select
                        value={f.fieldType}
                        onChange={(e) => updateFormField(i, { fieldType: e.target.value })}
                        className="h-8 rounded border border-slate-200 dark:border-slate-600 bg-white dark:bg-[#263244] px-2 text-sm text-slate-900 dark:text-indigo-100"
                      >
                        <option value="number">number</option>
                        <option value="string">string</option>
                      </select>
                      <select
                        value={f.unit}
                        onChange={(e) => updateFormField(i, { unit: Number(e.target.value) })}
                        className="h-8 rounded border border-slate-200 dark:border-slate-600 bg-white dark:bg-[#263244] px-2 text-sm text-slate-900 dark:text-indigo-100 w-36"
                      >
                        <option value={0}>None</option>
                        <option value={1}>Percent</option>
                        <option value={2}>Celsius</option>
                        <option value={3}>Volts</option>
                        <option value={4}>Watts</option>
                        <option value={5}>ZeroToOne</option>
                        <option value={6}>Mode</option>
                        <option value={7}>Level</option>
                      </select>
                      <Input
                        value={f.defaultValue ?? ""}
                        onChange={(e) => updateFormField(i, { defaultValue: e.target.value })}
                        placeholder="default"
                        className="bg-white dark:bg-[#263244] border-slate-200 dark:border-slate-600 w-24 h-8 text-sm"
                      />
                      <Button type="button" size="sm" variant="ghost" className="text-slate-700 dark:text-slate-400 hover:text-red-300 h-8 w-8 p-0" onClick={() => removeFormField(i)} aria-label="Remove field">
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  ))}
                </div>
              </div>
              <div className="flex gap-2 pt-2">
                <Button type="submit" disabled={templateSaving || !formType.trim()} className="bg-indigo-600 hover:bg-indigo-500 text-white">
                  {templateSaving ? "Saving…" : editingTemplateId ? "Update" : "Create"}
                </Button>
                <Button type="button" variant="outline" className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300" onClick={cancelTemplateForm}>
                  Cancel
                </Button>
              </div>
            </form>
          )}
        </div>
      </section>
      </>
      )}

      {activeTab === "simulator" && (
      <section className="mb-8">
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100 flex items-center gap-2">
          <Zap className="h-5 w-5 shrink-0" />
          Simulator
        </h2>
        <div className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600/50 dark:bg-[#1e2a3a] p-5 min-h-[120px]">
          <label className="block text-xs font-medium text-slate-700 dark:text-slate-400 mb-2 uppercase tracking-wide">
            Scenario
          </label>
          <select
            value={simScenario}
            onChange={(e) => setSimScenario(e.target.value as SimScenario)}
            className="mb-4 w-full max-w-xs rounded-lg border border-slate-200 dark:border-slate-600 bg-white dark:bg-[#263244] px-3 py-2 text-sm text-slate-900 dark:text-indigo-100 focus:border-indigo-500 focus:outline-none"
          >
            <option value="Normal">Normal</option>
            <option value="Mixed">Mixed</option>
            <option value="Anomaly">Anomaly</option>
          </select>
          <div className="flex flex-wrap gap-2">
            <Button
              onClick={handleSimStart}
              disabled={simLoading}
              className="bg-emerald-600 hover:bg-emerald-500 text-white"
            >
              Start generator
            </Button>
            <Button
              onClick={handleSimStop}
              disabled={simLoading}
              variant="outline"
              className="border-slate-200 dark:border-slate-600 text-slate-700 dark:text-slate-300"
            >
              Stop generator
            </Button>
          </div>
          {simError && (
            <p className="mt-3 text-sm text-red-400">{simError}</p>
          )}
        </div>
      </section>
      )}
    </CardWrap>
  );
};

export default SatelliteDetails;

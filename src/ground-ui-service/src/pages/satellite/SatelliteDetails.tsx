import { CommandsApiService } from "@/api/Services/CommandsApiService";
import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import {
  subscribeSatelliteEvents,
} from "@/api/Services/SseService";
import apiClient from "@/api/client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import type { CommandDto } from "@/types/command";
import type { DecisionDto } from "@/types/decision";
import type { MlResultDto } from "@/types/mlResult";
import type { SatelliteDto } from "@/types/satelliteApi";
import type { SseEvent } from "@/types/sse";
import {
  Activity,
  Cpu,
  Radio,
  Send,
  Shield,
  Zap,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const stateClass = (state: string) =>
  state === "Ok"
    ? "bg-emerald-500/20 text-emerald-300"
    : state === "Risk"
      ? "bg-amber-500/20 text-amber-300"
      : "bg-red-500/20 text-red-300";

const SatelliteDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [satellite, setSatellite] = useState<SatelliteDto | null>(null);
  const [decisions, setDecisions] = useState<DecisionDto[]>([]);
  const [mlResults, setMlResults] = useState<MlResultDto[]>([]);
  const [commands, setCommands] = useState<CommandDto[]>([]);
  const [events, setEvents] = useState<SseEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [cmdType, setCmdType] = useState("");
  const [cmdPriority, setCmdPriority] = useState(5);
  const [cmdTtl, setCmdTtl] = useState(300);
  const [cmdPayload, setCmdPayload] = useState("{}");
  const [simLoading, setSimLoading] = useState(false);
  const [simError, setSimError] = useState<string | null>(null);

  const loadSatellite = useCallback(async () => {
    if (!id) return;
    const s = await SatellitesApiService.get(id);
    setSatellite(s);
  }, [id]);

  const loadDecisions = useCallback(async () => {
    if (!id) return;
    const from = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
    const to = new Date().toISOString();
    const list = await SatellitesApiService.getDecisions(id, from, to, 20);
    setDecisions(list);
  }, [id]);

  const loadMlResults = useCallback(async () => {
    if (!id) return;
    const from = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
    const to = new Date().toISOString();
    const list = await SatellitesApiService.getMlResults(id, from, to);
    setMlResults(list);
  }, [id]);

  const loadCommands = useCallback(async () => {
    if (!id) return;
    const list = await CommandsApiService.listBySatellite(id);
    setCommands(list);
  }, [id]);

  useEffect(() => {
    if (!id) return;
    const run = async () => {
      setLoading(true);
      try {
        await loadSatellite();
        await loadDecisions();
        await loadMlResults();
        await loadCommands();
      } finally {
        setLoading(false);
      }
    };
    run();
  }, [id, loadSatellite, loadDecisions, loadMlResults, loadCommands]);

  useEffect(() => {
    if (!id) return;
    const unsubscribe = subscribeSatelliteEvents(id, (evt) => {
      setEvents((prev) => [evt, ...prev].slice(0, 50));
      if (
        evt.type === "decision.created" ||
        evt.type === "ml.result.created" ||
        evt.type === "command.created" ||
        evt.type === "command.claimed" ||
        evt.type === "command.executed" ||
        evt.type === "satellite.state.changed" ||
        evt.type === "link.window.open" ||
        evt.type === "link.window.close"
      ) {
        loadSatellite();
        loadDecisions();
        loadMlResults();
        loadCommands();
      }
    });
    return unsubscribe;
  }, [id, loadSatellite, loadDecisions, loadMlResults, loadCommands]);

  const handleCreateCommand = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !cmdType.trim()) return;
    try {
      await CommandsApiService.create(id, {
        type: cmdType.trim(),
        priority: cmdPriority,
        ttlSec: cmdTtl,
        payloadJson: cmdPayload.trim() || undefined,
      });
      await loadCommands();
      setCmdType("");
      setCmdPayload("{}");
    } catch (err) {
      console.error(err);
    }
  };

  const handleSimStart = async () => {
    if (!id) return;
    setSimLoading(true);
    setSimError(null);
    try {
      await apiClient.post(`satellites/${id}/sim/start`, {
        scenario: "normal",
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

  const latestMl = mlResults[0];
  const latestDecision = decisions[0];

  if (loading && !satellite) {
    return (
      <CardWrap>
        <Skeleton className="h-12 w-64 mb-4 bg-[#626a76]/40" />
        <Skeleton className="h-24 w-full mb-4 bg-[#626a76]/40" />
      </CardWrap>
    );
  }

  if (!satellite) {
    return (
      <CardWrap>
        <p className="text-slate-400">Satellite not found.</p>
        <Button onClick={() => navigate("/satellites")} className="mt-4">
          Back to satellites
        </Button>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-indigo-100">{satellite.name}</h1>
          <p className="mt-1 text-sm text-slate-400">
            State:{" "}
            <span className={stateClass(satellite.state)}>{satellite.state}</span>
            {" · "}
            Mode: {satellite.mode} · Link: {satellite.linkStatus}
            {satellite.lastBucketStart &&
              ` · Last bucket: ${new Date(satellite.lastBucketStart).toLocaleString()}`}
          </p>
          {satellite.missionName && (
            <p className="mt-1 text-sm text-slate-500">
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
          className="border-slate-600 text-slate-300"
        >
          Edit
        </Button>
      </div>

      {/* Telemetry widgets (placeholder: last bucket time; real values would need GET telemetry/bucket) */}
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Activity className="h-5 w-5" />
          Telemetry (last 1m bucket)
        </h2>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
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
              className="rounded-xl bg-[#1e2a3a] p-3 text-center"
            >
              <div className="text-xs text-slate-400 capitalize">
                {key.replace(/_/g, " ")}
              </div>
              <div className="mt-1 text-sm text-slate-300">
                {latestMl?.perSignalScore?.[key] != null
                  ? `score ${(latestMl.perSignalScore[key] * 100).toFixed(1)}%`
                  : "—"}
              </div>
            </div>
          ))}
        </div>
        {satellite.lastBucketStart && (
          <p className="mt-2 text-xs text-slate-500">
            Bucket start: {new Date(satellite.lastBucketStart).toLocaleString()}
          </p>
        )}
      </section>

      {/* ML panel */}
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Cpu className="h-5 w-5" />
          ML panel (Python)
        </h2>
        <div className="rounded-xl bg-[#1e2a3a] p-4">
          {latestMl ? (
            <>
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <span className="text-slate-400 text-sm">anomaly_score</span>
                  <div className="text-lg font-mono text-indigo-300">
                    {latestMl.anomalyScore.toFixed(4)}
                  </div>
                </div>
                <div>
                  <span className="text-slate-400 text-sm">confidence</span>
                  <div className="text-lg font-mono text-indigo-300">
                    {(latestMl.confidence * 100).toFixed(2)}%
                  </div>
                </div>
              </div>
              {latestMl.perSignalScore && (
                <div className="mb-4">
                  <span className="text-slate-400 text-sm block mb-2">
                    per_signal_score
                  </span>
                  <div className="flex flex-wrap gap-2">
                    {Object.entries(latestMl.perSignalScore).map(([k, v]) => (
                      <div
                        key={k}
                        className="rounded bg-[#263244] px-2 py-1 text-xs"
                      >
                        {k}: {(v * 100).toFixed(0)}%
                      </div>
                    ))}
                  </div>
                </div>
              )}
              {latestMl.topContributors && latestMl.topContributors.length > 0 && (
                <div className="mb-2">
                  <span className="text-slate-400 text-sm block mb-1">
                    top_contributors
                  </span>
                  <ul className="list-disc list-inside text-sm text-slate-300">
                    {latestMl.topContributors.map((c, i) => (
                      <li key={i}>
                        {c.key}: {(c.weight * 100).toFixed(1)}%
                      </li>
                    ))}
                  </ul>
                </div>
              )}
              <p className="text-xs text-slate-500">
                Model: {latestMl.modelName ?? "—"} /{" "}
                {latestMl.modelVersion ?? "—"}
              </p>
            </>
          ) : (
            <p className="text-slate-500">No ML result yet.</p>
          )}
        </div>
      </section>

      {/* Decision panel */}
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Shield className="h-5 w-5" />
          Decision panel (.NET)
        </h2>
        <div className="rounded-xl bg-[#1e2a3a] p-4">
          {latestDecision ? (
            <>
              <div className="flex items-center gap-2 mb-2">
                <span
                  className={`rounded px-2 py-0.5 text-sm font-medium ${stateClass(
                    latestDecision.type
                  )}`}
                >
                  {latestDecision.type}
                </span>
                <span className="text-slate-400 text-sm">
                  {new Date(latestDecision.createdAt).toLocaleString()}
                </span>
              </div>
              <p className="text-sm text-slate-300">{latestDecision.reason || "—"}</p>
            </>
          ) : (
            <p className="text-slate-500">No decision yet.</p>
          )}
        </div>
      </section>

      {/* Commands panel */}
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Send className="h-5 w-5" />
          Commands
        </h2>
        <form
          onSubmit={handleCreateCommand}
          className="mb-4 flex flex-wrap gap-3 items-end rounded-xl bg-[#1e2a3a] p-4"
        >
          <div>
            <label className="block text-xs text-slate-400 mb-1">Type</label>
            <Input
              value={cmdType}
              onChange={(e) => setCmdType(e.target.value)}
              placeholder="e.g. reboot"
              className="bg-[#263244] border-slate-600 w-32"
            />
          </div>
          <div>
            <label className="block text-xs text-slate-400 mb-1">Priority</label>
            <Input
              type="number"
              value={cmdPriority}
              onChange={(e) => setCmdPriority(Number(e.target.value))}
              className="bg-[#263244] border-slate-600 w-20"
            />
          </div>
          <div>
            <label className="block text-xs text-slate-400 mb-1">TTL (sec)</label>
            <Input
              type="number"
              value={cmdTtl}
              onChange={(e) => setCmdTtl(Number(e.target.value))}
              className="bg-[#263244] border-slate-600 w-24"
            />
          </div>
          <div>
            <label className="block text-xs text-slate-400 mb-1">Payload JSON</label>
            <Input
              value={cmdPayload}
              onChange={(e) => setCmdPayload(e.target.value)}
              placeholder="{}"
              className="bg-[#263244] border-slate-600 w-48 font-mono text-sm"
            />
          </div>
          <Button
            type="submit"
            disabled={!cmdType.trim()}
            className="bg-indigo-600 hover:bg-indigo-500 text-white"
          >
            Create command
          </Button>
        </form>
        <div className="overflow-x-auto rounded-xl bg-[#1e2a3a]">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-600 text-slate-400">
                <th className="pb-2 pr-4 font-medium">Type</th>
                <th className="pb-2 pr-4 font-medium">Status</th>
                <th className="pb-2 pr-4 font-medium">Priority</th>
                <th className="pb-2 pr-4 font-medium">TTL</th>
                <th className="pb-2 font-medium">Created</th>
              </tr>
            </thead>
            <tbody>
              {commands.slice(0, 20).map((c) => (
                <tr
                  key={c.id}
                  className="border-b border-slate-700/50 text-slate-300"
                >
                  <td className="py-2 pr-4">{c.type}</td>
                  <td className="py-2 pr-4">{c.status}</td>
                  <td className="py-2 pr-4">{c.priority}</td>
                  <td className="py-2 pr-4">{c.ttlSec}s</td>
                  <td className="py-2">
                    {new Date(c.createdAt).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {commands.length === 0 && (
            <p className="p-4 text-slate-500 text-sm">No commands.</p>
          )}
        </div>
      </section>

      {/* Simulator panel */}
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Zap className="h-5 w-5" />
          Simulator
        </h2>
        <div className="rounded-xl bg-[#1e2a3a] p-4">
          <p className="text-sm text-slate-400 mb-3">
            Telemetry scenario: normal (overheating, battery_drain, comms_drop
            can be added via backend).
          </p>
          <div className="flex gap-2">
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
              className="border-slate-600 text-slate-300"
            >
              Stop generator
            </Button>
          </div>
          {simError && (
            <p className="mt-2 text-sm text-red-400">{simError}</p>
          )}
        </div>
      </section>

      {/* Activity feed */}
      <section>
        <h2 className="mb-3 text-lg font-medium text-indigo-100 flex items-center gap-2">
          <Radio className="h-5 w-5" />
          Activity feed
        </h2>
        <div className="max-h-64 overflow-y-auto rounded-xl bg-[#1e2a3a] p-3 space-y-2">
          {events.length === 0 ? (
            <p className="text-sm text-slate-500">No events yet.</p>
          ) : (
            events.map((evt) => (
              <div
                key={evt.eventId}
                className="rounded-lg bg-[#263244] px-3 py-2 text-sm"
              >
                <span className="font-medium text-indigo-300">{evt.type}</span>
                <span className="ml-2 text-slate-400">{evt.ts}</span>
                {evt.payload != null && (
                  <pre className="mt-1 overflow-x-auto text-xs text-slate-500">
                    {JSON.stringify(evt.payload)}
                  </pre>
                )}
              </div>
            ))
          )}
        </div>
      </section>
    </CardWrap>
  );
};

export default SatelliteDetails;

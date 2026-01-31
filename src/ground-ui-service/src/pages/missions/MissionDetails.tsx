import { CommandsApiService } from "@/api/Services/CommandsApiService";
import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { CommandDto } from "@/types/command";
import type { Mission } from "@/types/mission";
import type { SatelliteDto } from "@/types/satelliteApi";
import type { SseEvent } from "@/types/sse";
import { subscribeMissionEvents } from "@/api/Services/SseService";
import { Braces, Link2, ListOrdered, MapPin, Pencil, Satellite as SatelliteIcon, Activity } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { MapContainer, Marker, Popup, TileLayer, useMap } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { formatUtcInUserTz } from "@/lib/dateUtils";
import CardWrap from "../Components/CardWrap";

const COMMAND_STATUSES = ["Queued", "Claimed", "Executed", "Failed", "Canceled", "Expired"];
const DEFAULT_MAP_CENTER: [number, number] = [20, 0];
const DEFAULT_MAP_ZOOM = 2;

interface SatellitePosition {
  lat: number;
  lon: number;
  altKm?: number;
  ts?: string;
}

function MapCenterOnPositions({ positions }: { positions: Record<string, SatellitePosition> }) {
  const map = useMap();
  const fittedOnce = useRef(false);
  const coords = useMemo(
    () => Object.values(positions).filter((p) => p != null && (p.lat !== 0 || p.lon !== 0)),
    [positions]
  );
  useEffect(() => {
    if (coords.length === 0 || fittedOnce.current) return;
    fittedOnce.current = true;
    const bounds = L.latLngBounds(coords.map((c) => [c.lat, c.lon] as L.LatLngExpression));
    map.fitBounds(bounds, { padding: [40, 40], maxZoom: 8 });
  }, [map, coords]);
  return null;
}

function formatPayloadValue(v: unknown): string {
  if (v == null) return "—";
  if (typeof v === "number") return Number.isInteger(v) ? String(v) : v.toFixed(4);
  if (typeof v === "boolean") return v ? "true" : "false";
  if (typeof v === "object") {
    const s = JSON.stringify(v);
    return s.length > 80 ? s.slice(0, 80) + "…" : s;
  }
  const s = String(v);
  return s.length > 80 ? s.slice(0, 80) + "…" : s;
}

function isLocationObject(v: unknown): v is { lat?: number; lon?: number; altKm?: number; alt?: number } {
  if (v == null || typeof v !== "object") return false;
  const o = v as Record<string, unknown>;
  return ("lat" in o && typeof o.lat === "number") || ("lon" in o && typeof o.lon === "number");
}

function formatLocation(loc: { lat?: number; lon?: number; altKm?: number; alt?: number }): string {
  const parts: string[] = [];
  if (loc.lat != null) parts.push(`lat ${Number(loc.lat).toFixed(4)}°`);
  if (loc.lon != null) parts.push(`lon ${Number(loc.lon).toFixed(4)}°`);
  const alt = loc.altKm ?? loc.alt;
  if (alt != null) parts.push(`alt ${Number(alt).toFixed(0)} km`);
  return parts.length ? parts.join(" · ") : "—";
}

function isMlResultPayload(p: unknown): p is Record<string, unknown> {
  if (p == null || typeof p !== "object") return false;
  const o = p as Record<string, unknown>;
  return (
    (typeof o.anomaly_score === "number" || typeof o.anomalyScore === "number") &&
    (typeof o.confidence === "number" || typeof o.confidence === "number")
  );
}

function MlResultPayloadView({ payload }: { payload: Record<string, unknown> }) {
  const anomalyScore = (payload.anomaly_score ?? payload.anomalyScore) as number | undefined;
  const confidence = (payload.confidence ?? payload.confidence) as number | undefined;
  const perSignal =
    (payload.per_signal_score ?? payload.perSignalScore) as Record<string, number> | undefined;
  const topContributors = (payload.top_contributors ?? payload.topContributors) as
    | Array<{ key: string; weight: number }>
    | undefined;
  const modelName = (payload.model_name ?? payload.modelName) as string | undefined;
  const modelVersion = (payload.model_version ?? payload.modelVersion) as string | undefined;

  return (
    <div className="mt-3 space-y-3">
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
            per signal
          </span>
          <div className="mt-1.5 flex flex-wrap gap-2">
            {Object.entries(perSignal).map(([k, v]) => (
              <span
                key={k}
                className="rounded-md border border-slate-200 dark:border-slate-600/50 bg-white dark:bg-[#263244] px-2 py-1 text-xs font-mono text-slate-800 dark:text-slate-200"
              >
                {k.replace(/_/g, " ")}: {(Number(v) * 100).toFixed(0)}%
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
      {(modelName != null || modelVersion != null) && (
        <p className="border-t border-slate-200 dark:border-slate-600/50 pt-2 text-[10px] text-slate-500 dark:text-slate-500">
          Model: {modelName ?? "—"} / {modelVersion ?? "—"}
        </p>
      )}
    </div>
  );
}

const MissionDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [mission, setMission] = useState<Mission | null>(null);
  const [satellites, setSatellites] = useState<SatelliteDto[]>([]);
  const [events, setEvents] = useState<SseEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [attachOpen, setAttachOpen] = useState(false);
  const [allSatellites, setAllSatellites] = useState<SatelliteDto[]>([]);
  const [expandedJsonId, setExpandedJsonId] = useState<string | null>(null);
  const [feedFilterSatelliteIds, setFeedFilterSatelliteIds] = useState<string[]>([]);
  const [lastPositions, setLastPositions] = useState<Record<string, SatellitePosition>>({});
  const [commands, setCommands] = useState<CommandDto[]>([]);
  const [commandsLoading, setCommandsLoading] = useState(false);
  const [commandStatusFilter, setCommandStatusFilter] = useState("");
  const [activeTab, setActiveTab] = useState<"satellites" | "map" | "commands" | "activity">("satellites");

  const loadMission = useCallback(async () => {
    if (!id) return;
    const m = await MissionsApiService.get(id);
    setMission(m);
  }, [id]);

  const loadSatellites = useCallback(async () => {
    if (!id) return;
    const list = await SatellitesApiService.list(id);
    setSatellites(list);
  }, [id]);

  useEffect(() => {
    if (!id) return;
    const run = async () => {
      setLoading(true);
      try {
        await loadMission();
        await loadSatellites();
        const all = await SatellitesApiService.list();
        setAllSatellites(all);
      } finally {
        setLoading(false);
      }
    };
    run();
  }, [id, loadMission, loadSatellites]);

  const loadCommands = useCallback(async () => {
    if (!id) return;
    setCommandsLoading(true);
    try {
      const list = await CommandsApiService.listByMission(id, commandStatusFilter || undefined);
      setCommands(list);
    } finally {
      setCommandsLoading(false);
    }
  }, [id, commandStatusFilter]);

  useEffect(() => {
    if (!id) return;
    loadCommands();
  }, [id, commandStatusFilter, loadCommands]);

  useEffect(() => {
    if (!id) return;
    const unsubscribe = subscribeMissionEvents(id, (evt) => {
      setEvents((prev) => [evt, ...prev].slice(0, 50));
      if (evt.type === "telemetry" && evt.satelliteId && evt.payload) {
        const p = evt.payload as {
          location?: { lat?: number; lon?: number; altKm?: number; Lat?: number; Lon?: number; AltKm?: number };
          timestamp?: string;
        };
        const loc = p?.location;
        if (loc != null) {
          const lat = loc.lat ?? (loc as { Lat?: number }).Lat;
          const lon = loc.lon ?? (loc as { Lon?: number }).Lon;
          if (typeof lat === "number" && typeof lon === "number") {
            const sid = String(evt.satelliteId);
            const altKm = loc.altKm ?? (loc as { AltKm?: number }).AltKm;
            setLastPositions((prev) => ({
              ...prev,
              [sid]: { lat, lon, altKm, ts: p?.timestamp ?? evt.ts },
            }));
          }
        }
      }
      if (
        evt.type === "telemetry.bucket.created" ||
        evt.type === "ml.result.created" ||
        evt.type === "decision.created" ||
        evt.type === "command.created" ||
        evt.type === "command.claimed" ||
        evt.type === "command.executed"
      ) {
        loadSatellites();
        loadCommands();
      }
    });
    return unsubscribe;
  }, [id, loadSatellites, loadCommands]);

  const handleAttach = async (satelliteId: string) => {
    if (!id) return;
    await MissionsApiService.attachSatellite(id, satelliteId);
    await loadSatellites();
    setAllSatellites(await SatellitesApiService.list());
    setAttachOpen(false);
  };

  const handleDetach = async (satelliteId: string) => {
    if (!id) return;
    await MissionsApiService.detachSatellite(id, satelliteId);
    await loadSatellites();
    setAllSatellites(await SatellitesApiService.list());
  };

  const okCount = satellites.filter((s) => s.state === "Ok").length;
  const riskCount = satellites.filter((s) => s.state === "Risk").length;
  const problemCount = satellites.filter((s) => s.state === "Problem").length;

  const filteredEvents =
    feedFilterSatelliteIds.length === 0
      ? events
      : events.filter(
          (evt) => evt.satelliteId && feedFilterSatelliteIds.includes(evt.satelliteId)
        );

  const toggleFeedFilterSatellite = (satelliteId: string) => {
    setFeedFilterSatelliteIds((prev) =>
      prev.includes(satelliteId)
        ? prev.filter((id) => id !== satelliteId)
        : [...prev, satelliteId]
    );
  };

  const selectAllFeedFilter = () => setFeedFilterSatelliteIds([]);

  const getSatelliteName = (satelliteId: string | null | undefined) =>
    satelliteId
      ? satellites.find((s) => s.id === satelliteId)?.name ?? satelliteId.slice(0, 8) + "…"
      : "—";

  if (loading && !mission) {
    return (
      <CardWrap>
        <Skeleton className="h-12 w-64 mb-4 bg-slate-300 dark:bg-[#626a76]/40" />
        <Skeleton className="h-24 w-full mb-4 bg-slate-300 dark:bg-[#626a76]/40" />
      </CardWrap>
    );
  }

  if (!mission) {
    return (
      <CardWrap>
        <p className="text-slate-700 dark:text-slate-400">Mission not found.</p>
        <Button
          onClick={() => navigate("/missions")}
          className="mt-4 min-h-10 px-4 py-2.5"
        >
          Back to missions
        </Button>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-slate-900 dark:text-indigo-100">{mission.name}</h1>
          <p className="mt-1 text-sm text-slate-700 dark:text-slate-400">
            Status: {mission.isActive ? "Active" : "Inactive"} · Created{" "}
            {formatUtcInUserTz(mission.createdAt)}
          </p>
          {mission.description && (
            <p className="mt-2 text-slate-700 dark:text-slate-300">{mission.description}</p>
          )}
        </div>
        <Button
          variant="outline"
          size="default"
          onClick={() => navigate(`/missions/${id}/edit`)}
          className="min-h-10 min-w-[100px] px-4 py-2.5 border-slate-300 text-slate-700 dark:border-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700/50"
        >
          <Pencil className="mr-2 h-4 w-4 shrink-0" />
          Edit
        </Button>
      </div>

      <div className="mb-6 grid grid-cols-3 gap-4">
        <div className="rounded-xl bg-emerald-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-300">{okCount}</div>
          <div className="text-xs text-slate-700 dark:text-slate-400">OK</div>
        </div>
        <div className="rounded-xl bg-amber-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-amber-600 dark:text-amber-300">{riskCount}</div>
          <div className="text-xs text-slate-700 dark:text-slate-400">Risk</div>
        </div>
        <div className="rounded-xl bg-red-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-red-600 dark:text-red-300">{problemCount}</div>
          <div className="text-xs text-slate-700 dark:text-slate-400">Problem</div>
        </div>
      </div>

      <div className="mb-4 flex rounded-xl border border-slate-200 dark:border-slate-600 overflow-hidden bg-slate-50 dark:bg-[#1e2a3a] p-1">
        {(
          [
            { id: "satellites" as const, label: "Satellites", icon: SatelliteIcon },
            { id: "map" as const, label: "Map", icon: MapPin },
            { id: "commands" as const, label: "Command queue", icon: ListOrdered },
            { id: "activity" as const, label: "Activity", icon: Activity },
          ] as const
        ).map(({ id, label, icon: Icon }) => (
          <button
            key={id}
            type="button"
            data-tab={id}
            onClick={() => setActiveTab(id)}
            className={`min-h-10 flex-1 flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors ${
              activeTab === id
                ? "bg-indigo-600 text-white shadow-sm"
                : "text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700/50"
            }`}
          >
            <Icon className="h-4 w-4 shrink-0" />
            {label}
          </button>
        ))}
      </div>

      {activeTab === "satellites" && (
      <section className="mb-6">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-medium text-slate-900 dark:text-indigo-100">
            Satellites in mission
          </h2>
          <div className="flex flex-wrap gap-3">
            <Button
              size="default"
              onClick={() => setAttachOpen(true)}
              className="min-h-10 px-4 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white"
            >
              <Link2 className="mr-2 h-4 w-4 shrink-0" />
              Attach satellite
            </Button>
          </div>
        </div>
        {attachOpen && (
          <div className="mb-4 rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a] p-4">
            <p className="mb-2 text-sm text-slate-700 dark:text-slate-400">
              Select a satellite to attach (or already in another mission):
            </p>
            <div className="flex flex-wrap gap-3">
              {allSatellites.map((s) => (
                <Button
                  key={s.id}
                  size="default"
                  variant="outline"
                  disabled={s.missionId === id}
                  onClick={() =>
                    s.missionId === id ? undefined : handleAttach(s.id)
                  }
                  className="min-h-10 px-4 py-2.5 border-slate-300 text-slate-700 dark:border-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700/50 disabled:opacity-50"
                >
                  {s.name}
                  {s.missionId === id ? " (attached)" : ""}
                </Button>
              ))}
              {allSatellites.length === 0 && (
                <p className="text-sm text-slate-700 dark:text-slate-500">No satellites available.</p>
              )}
            </div>
            <Button
              size="default"
              variant="ghost"
              className="mt-3 min-h-10 px-4 py-2.5 text-slate-600 dark:text-slate-400 hover:bg-slate-200 dark:hover:bg-slate-700/30 dark:hover:text-slate-200"
              onClick={() => setAttachOpen(false)}
            >
              Close
            </Button>
          </div>
        )}
        {satellites.length === 0 ? (
          <p className="text-slate-700 dark:text-slate-500">No satellites attached.</p>
        ) : (
          <div className="space-y-2">
            {satellites.map((s) => (
              <div
                key={s.id}
                className="flex items-center justify-between rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a] p-4 min-h-[56px]"
              >
                <div
                  className="flex cursor-pointer items-center gap-3 flex-1 min-w-0"
                  onClick={() => navigate(`/satellites/${s.id}`)}
                >
                  <SatelliteIcon className="h-5 w-5 text-indigo-400 shrink-0" />
                  <span className="font-medium text-slate-900 dark:text-indigo-100">{s.name}</span>
                  <span
                    className={`rounded-md px-2.5 py-1 text-xs font-medium ${
                      s.state === "Ok"
                        ? "bg-emerald-500/20 text-emerald-300"
                        : s.state === "Risk"
                          ? "bg-amber-500/20 text-amber-300"
                          : "bg-red-500/20 text-red-300"
                    }`}
                  >
                    {s.state}
                  </span>
                  <span className="text-xs text-slate-700 dark:text-slate-500">{s.linkStatus}</span>
                </div>
                <Button
                  size="default"
                  variant="ghost"
                  className="min-h-10 px-4 py-2.5 text-slate-600 dark:text-slate-400 hover:bg-slate-200 dark:hover:bg-slate-700/50 dark:hover:text-slate-200 shrink-0"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleDetach(s.id);
                  }}
                >
                  Detach
                </Button>
              </div>
            ))}
          </div>
        )}
      </section>
      )}

      {activeTab === "map" && (
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-slate-900 dark:text-indigo-100">Map</h2>
        <p className="mb-3 text-sm text-slate-700 dark:text-slate-400">
          Positions from live telemetry. Start simulator on a satellite to see movement.
        </p>
        <div className="rounded-xl overflow-hidden border border-slate-200 dark:border-slate-600/50 bg-slate-100 dark:bg-[#1e2a3a] h-[400px]">
          <MapContainer
            center={DEFAULT_MAP_CENTER}
            zoom={DEFAULT_MAP_ZOOM}
            className="h-full w-full"
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <MapCenterOnPositions positions={lastPositions} />
            {satellites.map((s) => {
              const pos = lastPositions[String(s.id)];
              if (pos == null || (pos.lat === 0 && pos.lon === 0)) return null;
              const icon = L.divIcon({
                className: "custom-marker",
                html: `<span class="flex items-center justify-center w-8 h-8 rounded-full bg-indigo-500/90 border-2 border-indigo-300 text-white text-xs font-bold shadow-lg">${s.name.slice(-1) || "?"}</span>`,
                iconSize: [32, 32],
                iconAnchor: [16, 16],
              });
              return (
                <Marker key={s.id} position={[pos.lat, pos.lon]} icon={icon}>
                  <Popup>
                    <div className="text-sm">
                      <div className="font-medium text-slate-800 dark:text-slate-200">{s.name}</div>
                      <div className="text-slate-700 dark:text-slate-400 mt-0.5">
                        {pos.lat.toFixed(4)}°, {pos.lon.toFixed(4)}°
                        {pos.altKm != null ? ` · ${pos.altKm.toFixed(0)} km` : ""}
                      </div>
                      {pos.ts && (
                        <div className="text-xs text-slate-600 dark:text-slate-500 mt-1">{formatUtcInUserTz(pos.ts)}</div>
                      )}
                      <Link
                        to={`/satellites/${s.id}`}
                        className="mt-2 inline-block w-full text-center rounded bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm py-1.5 px-2"
                      >
                        Open satellite
                      </Link>
                    </div>
                  </Popup>
                </Marker>
              );
            })}
          </MapContainer>
        </div>
        <div className="mt-2 flex flex-wrap gap-2 items-center text-sm text-slate-700 dark:text-slate-400">
          <MapPin className="h-4 w-4 shrink-0" />
          {satellites.length === 0 ? (
            <span>No satellites in mission. Attach satellites above.</span>
          ) : Object.keys(lastPositions).length === 0 ? (
            <span>No telemetry yet. Start the simulator on a satellite to see positions.</span>
          ) : (
            <span>
              Showing {Object.keys(lastPositions).length} of {satellites.length} satellites with recent position.
            </span>
          )}
        </div>
      </section>
      )}

      {activeTab === "commands" && (
      <section className="mb-6">
        <h2 className="mb-3 text-lg font-medium text-slate-900 dark:text-indigo-100">Command queue</h2>
        <div className="mb-3 flex flex-wrap items-center gap-3">
          <span className="text-sm font-medium text-slate-700 dark:text-slate-400">Filter by status:</span>
          <select
            value={commandStatusFilter}
            onChange={(e) => setCommandStatusFilter(e.target.value)}
            className="min-h-10 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm text-slate-900 dark:border-slate-600 dark:bg-[#1e2a3a] dark:text-slate-200"
          >
            <option value="">All</option>
            {COMMAND_STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
        {commandsLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-12 w-full rounded-xl bg-slate-300 dark:bg-[#626a76]/40" />
            <Skeleton className="h-12 w-full rounded-xl bg-slate-300 dark:bg-[#626a76]/40" />
            <Skeleton className="h-12 w-full rounded-xl bg-slate-300 dark:bg-[#626a76]/40" />
          </div>
        ) : (
          <div className="w-full overflow-x-auto rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a]">
            <table className="w-full min-w-[640px] text-left text-sm">
              <thead>
                <tr className="border-b border-slate-200 dark:border-slate-600">
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">Type</th>
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">Status</th>
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">Priority</th>
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">TTL</th>
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">Created</th>
                  <th className="px-4 py-3 font-medium text-slate-700 dark:text-slate-400">Executed</th>
                </tr>
              </thead>
              <tbody>
                {commands.map((c) => (
                  <tr
                    key={c.id}
                    className="border-b border-slate-200 last:border-0 dark:border-slate-700/50"
                  >
                    <td className="px-4 py-3 font-medium text-slate-900 dark:text-indigo-100">{c.type}</td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300">{c.status}</td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300">{c.priority}</td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300">{c.ttlSec}s</td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300">
                      {new Date(c.createdAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-slate-700 dark:text-slate-300">
                      {c.executedAt ? new Date(c.executedAt).toLocaleString() : "—"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {commands.length === 0 && (
              <p className="p-6 text-center text-slate-600 dark:text-slate-500">No commands.</p>
            )}
          </div>
        )}
      </section>
      )}

      {activeTab === "activity" && (
      <section>
        <h2 className="mb-4 text-lg font-medium text-slate-900 dark:text-indigo-100">
          Activity feed
        </h2>
        <div className="mb-3 flex flex-wrap items-center gap-2">
          <span className="text-xs font-medium text-slate-600 dark:text-slate-400 uppercase tracking-wide">
            Filter by satellite:
          </span>
          <button
            type="button"
            onClick={selectAllFeedFilter}
            className={`min-h-9 px-3 py-1.5 rounded-xl text-sm font-medium transition-colors ${
              feedFilterSatelliteIds.length === 0
                ? "bg-indigo-600 text-white"
                : "bg-slate-300 text-slate-600 dark:bg-slate-600/50 dark:text-slate-300 hover:bg-slate-400 dark:hover:bg-slate-600"
            }`}
          >
            All
          </button>
          {satellites.map((s) => {
            const isSelected =
              feedFilterSatelliteIds.length === 0 || feedFilterSatelliteIds.includes(s.id);
            return (
              <button
                key={s.id}
                type="button"
                onClick={() => toggleFeedFilterSatellite(s.id)}
                className={`min-h-9 px-3 py-1.5 rounded-xl text-sm font-medium transition-colors ${
                  isSelected
                    ? "bg-indigo-600 text-white hover:bg-indigo-500 dark:bg-indigo-600/80 dark:hover:bg-indigo-500/80"
                    : "bg-slate-200 text-slate-600 dark:bg-slate-600/30 dark:text-slate-500 hover:bg-slate-300 dark:hover:bg-slate-600/50"
                }`}
              >
                {s.name}
              </button>
            );
          })}
          {satellites.length === 0 && (
            <span className="text-sm text-slate-700 dark:text-slate-500">Attach satellites to filter.</span>
          )}
        </div>
        <div className="max-h-[420px] overflow-y-auto rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a] p-4 space-y-3">
          {filteredEvents.length === 0 ? (
            <p className="py-8 text-center text-slate-700 dark:text-slate-500">
              {events.length === 0 ? "No events yet." : "No events for selected satellites."}
            </p>
          ) : (
            filteredEvents.map((evt) => (
              <div
                key={evt.eventId}
                className="rounded-xl border border-slate-200 bg-white dark:border-slate-600/40 dark:bg-[#263244] px-4 py-4 min-h-[72px] shadow-sm"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2 mb-1">
                      <span className="inline-flex items-center rounded-md bg-slate-200 px-2 py-0.5 text-xs font-medium text-slate-700 dark:bg-slate-600/60 dark:text-slate-300">
                        {getSatelliteName(evt.satelliteId)}
                      </span>
                      <span className="font-semibold text-slate-900 dark:text-indigo-200">{evt.type}</span>
                      <span className="text-sm text-slate-700 dark:text-slate-400">
                        {formatUtcInUserTz(evt.ts)}
                      </span>
                    </div>
                    {evt.payload != null && (
                      <>
                        {expandedJsonId === evt.eventId ? (
                          <pre className="mt-3 rounded-lg bg-slate-100 dark:bg-[#1e2a3a] p-3 text-xs text-slate-700 dark:text-slate-400 overflow-x-auto max-h-48">
                            {JSON.stringify(evt.payload, null, 2)}
                          </pre>
                        ) : isMlResultPayload(evt.payload) ? (
                          <MlResultPayloadView payload={evt.payload as Record<string, unknown>} />
                        ) : (
                          <div className="mt-3 space-y-1.5">
                            {Object.entries(evt.payload as Record<string, unknown>).map(([key, val]) => {
                              const isLocation = (key === "location" || key === "location_deg") && isLocationObject(val);
                              return (
                                <div key={key} className="flex flex-wrap items-baseline gap-x-2 text-xs">
                                  <span className="font-medium text-slate-600 dark:text-slate-400 shrink-0">
                                    {key.replace(/_/g, " ")}:
                                  </span>
                                  <span className="text-slate-800 dark:text-slate-200 font-mono break-all">
                                    {isLocation ? formatLocation(val) : formatPayloadValue(val)}
                                  </span>
                                </div>
                              );
                            })}
                          </div>
                        )}
                        <button
                          type="button"
                          onClick={() =>
                            setExpandedJsonId((prev) =>
                              prev === evt.eventId ? null : evt.eventId
                            )
                          }
                          className="mt-2 inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[10px] font-medium text-slate-500 dark:text-slate-500 hover:text-slate-700 dark:hover:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700/50 transition-colors"
                          title={expandedJsonId === evt.eventId ? "Hide JSON" : "Show as JSON"}
                        >
                          <Braces className="h-2.5 w-2.5 shrink-0" />
                          {expandedJsonId === evt.eventId ? "hide" : "json"}
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </section>
      )}
    </CardWrap>
  );
};

export default MissionDetails;

import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { Mission } from "@/types/mission";
import type { SatelliteDto } from "@/types/satelliteApi";
import type { SseEvent } from "@/types/sse";
import { subscribeMissionEvents } from "@/api/Services/SseService";
import { Link2, Pencil, Satellite as SatelliteIcon } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const MissionDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [mission, setMission] = useState<Mission | null>(null);
  const [satellites, setSatellites] = useState<SatelliteDto[]>([]);
  const [events, setEvents] = useState<SseEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [attachOpen, setAttachOpen] = useState(false);
  const [allSatellites, setAllSatellites] = useState<SatelliteDto[]>([]);

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

  useEffect(() => {
    if (!id) return;
    const unsubscribe = subscribeMissionEvents(id, (evt) => {
      setEvents((prev) => [evt, ...prev].slice(0, 50));
      if (
        evt.type === "telemetry.bucket.created" ||
        evt.type === "ml.result.created" ||
        evt.type === "decision.created" ||
        evt.type === "command.created" ||
        evt.type === "command.claimed" ||
        evt.type === "command.executed"
      ) {
        loadSatellites();
      }
    });
    return unsubscribe;
  }, [id, loadSatellites]);

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

  if (loading && !mission) {
    return (
      <CardWrap>
        <Skeleton className="h-12 w-64 mb-4 bg-[#626a76]/40" />
        <Skeleton className="h-24 w-full mb-4 bg-[#626a76]/40" />
      </CardWrap>
    );
  }

  if (!mission) {
    return (
      <CardWrap>
        <p className="text-slate-400">Mission not found.</p>
        <Button onClick={() => navigate("/missions")} className="mt-4">
          Back to missions
        </Button>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-indigo-100">{mission.name}</h1>
          <p className="mt-1 text-sm text-slate-400">
            Status: {mission.isActive ? "Active" : "Inactive"} · Created{" "}
            {new Date(mission.createdAt).toLocaleString()}
          </p>
          {mission.description && (
            <p className="mt-2 text-slate-300">{mission.description}</p>
          )}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigate(`/missions/${id}/edit`)}
          className="border-slate-600 text-slate-300"
        >
          <Pencil className="mr-2 h-4 w-4" />
          Edit
        </Button>
      </div>

      <div className="mb-6 grid grid-cols-3 gap-4">
        <div className="rounded-xl bg-emerald-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-emerald-300">{okCount}</div>
          <div className="text-xs text-slate-400">OK</div>
        </div>
        <div className="rounded-xl bg-amber-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-amber-300">{riskCount}</div>
          <div className="text-xs text-slate-400">Risk</div>
        </div>
        <div className="rounded-xl bg-red-500/10 p-4 text-center">
          <div className="text-2xl font-bold text-red-300">{problemCount}</div>
          <div className="text-xs text-slate-400">Problem</div>
        </div>
      </div>

      <section className="mb-6">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-medium text-indigo-100">
            Satellites in mission
          </h2>
          <div className="flex gap-2">
            <Button
              size="sm"
              onClick={() => setAttachOpen(true)}
              className="bg-indigo-600 hover:bg-indigo-500 text-white"
            >
              <Link2 className="mr-2 h-4 w-4" />
              Attach satellite
            </Button>
            <Button
              size="sm"
              variant="outline"
              onClick={() => navigate(`/missions/${id}/commands`)}
              className="border-slate-600 text-slate-300"
            >
              Command queue
            </Button>
          </div>
        </div>
        {attachOpen && (
          <div className="mb-4 rounded-xl bg-[#1e2a3a] p-4">
            <p className="mb-2 text-sm text-slate-400">
              Select a satellite to attach (or already in another mission):
            </p>
            <div className="flex flex-wrap gap-2">
              {allSatellites.map((s) => (
                <Button
                  key={s.id}
                  size="sm"
                  variant="outline"
                  disabled={s.missionId === id}
                  onClick={() =>
                    s.missionId === id ? undefined : handleAttach(s.id)
                  }
                  className="border-slate-600 text-slate-300"
                >
                  {s.name}
                  {s.missionId === id ? " (attached)" : ""}
                </Button>
              ))}
              {allSatellites.length === 0 && (
                <p className="text-sm text-slate-500">No satellites available.</p>
              )}
            </div>
            <Button
              size="sm"
              variant="ghost"
              className="mt-2 text-slate-400"
              onClick={() => setAttachOpen(false)}
            >
              Close
            </Button>
          </div>
        )}
        {satellites.length === 0 ? (
          <p className="text-slate-500">No satellites attached.</p>
        ) : (
          <div className="space-y-2">
            {satellites.map((s) => (
              <div
                key={s.id}
                className="flex items-center justify-between rounded-xl bg-[#1e2a3a] p-3"
              >
                <div
                  className="flex cursor-pointer items-center gap-3"
                  onClick={() => navigate(`/satellites/${s.id}`)}
                >
                  <SatelliteIcon className="h-5 w-5 text-indigo-400" />
                  <span className="font-medium text-indigo-100">{s.name}</span>
                  <span
                    className={`rounded px-2 py-0.5 text-xs ${
                      s.state === "Ok"
                        ? "bg-emerald-500/20 text-emerald-300"
                        : s.state === "Risk"
                          ? "bg-amber-500/20 text-amber-300"
                          : "bg-red-500/20 text-red-300"
                    }`}
                  >
                    {s.state}
                  </span>
                  <span className="text-xs text-slate-500">{s.linkStatus}</span>
                </div>
                <Button
                  size="sm"
                  variant="ghost"
                  className="text-slate-400"
                  onClick={() => handleDetach(s.id)}
                >
                  Detach
                </Button>
              </div>
            ))}
          </div>
        )}
      </section>

      <section>
        <h2 className="mb-3 text-lg font-medium text-indigo-100">
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

export default MissionDetails;

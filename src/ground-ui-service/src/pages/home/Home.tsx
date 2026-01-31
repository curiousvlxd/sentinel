import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { Button } from "@/components/ui/button";
import type { Mission } from "@/types/mission";
import type { SatelliteDto } from "@/types/satelliteApi";
import { Rocket, Satellite as SatelliteIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const Home = () => {
  const [missions, setMissions] = useState<Mission[]>([]);
  const [satellites, setSatellites] = useState<SatelliteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const load = async () => {
      setApiError(null);
      try {
        const [m, s] = await Promise.all([
          MissionsApiService.list(),
          SatellitesApiService.list(),
        ]);
        setMissions(m);
        setSatellites(s);
      } catch (err) {
        setMissions([]);
        setSatellites([]);
        const msg = err && typeof err === "object" && "message" in err
          ? String((err as Error).message)
          : "Failed to load data";
        setApiError(msg);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return (
    <CardWrap>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-indigo-100">Ground Control</h1>
        <p className="mt-1 text-slate-700 dark:text-slate-400">
          Missions and satellites overview
        </p>
      </div>

      {apiError && (
        <div className="mb-4 rounded-xl bg-red-500/10 border border-red-500/30 px-4 py-3 text-sm text-red-300">
          API error: {apiError}
        </div>
      )}
      {loading ? (
        <div className="flex justify-center py-12">
          <div className="h-10 w-10 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent" />
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2">
          <div className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a] p-4">
            <div className="mb-3 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-slate-900 dark:text-indigo-100 flex items-center gap-2">
                <Rocket className="h-5 w-5" />
                Missions
              </h2>
              <Button
                size="sm"
                onClick={() => navigate("/missions")}
                className="bg-indigo-600 hover:bg-indigo-500 text-white"
              >
                View all
              </Button>
            </div>
            {missions.length === 0 ? (
              <p className="text-sm text-slate-600 dark:text-slate-500">No missions.</p>
            ) : (
              <ul className="space-y-2">
                {missions.slice(0, 5).map((m) => (
                  <li key={m.id}>
                    <button
                      type="button"
                      onClick={() => navigate(`/missions/${m.id}`)}
                      className="text-left w-full rounded-lg px-3 py-2 text-sm text-slate-900 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-[#263244] transition"
                    >
                      {m.name}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="rounded-xl border border-slate-200 bg-slate-50 dark:border-slate-600 dark:bg-[#1e2a3a] p-4">
            <div className="mb-3 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-slate-900 dark:text-indigo-100 flex items-center gap-2">
                <SatelliteIcon className="h-5 w-5" />
                Satellites
              </h2>
              <Button
                size="sm"
                onClick={() => navigate("/satellites")}
                className="bg-indigo-600 hover:bg-indigo-500 text-white"
              >
                View all
              </Button>
            </div>
            {satellites.length === 0 ? (
              <p className="text-sm text-slate-600 dark:text-slate-500">No satellites.</p>
            ) : (
              <ul className="space-y-2">
                {satellites.slice(0, 5).map((s) => (
                  <li key={s.id}>
                    <button
                      type="button"
                      onClick={() => navigate(`/satellites/${s.id}`)}
                      className="text-left w-full rounded-lg px-3 py-2 text-sm text-slate-900 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-[#263244] transition flex items-center justify-between"
                    >
                      <span>{s.name}</span>
                      <span className="text-xs text-slate-600 dark:text-slate-500">{s.state}</span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </CardWrap>
  );
};

export default Home;

import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { SatelliteDto } from "@/types/satelliteApi";
import { Plus, Satellite as SatelliteIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { formatUtcInUserTz } from "@/lib/dateUtils";
import CardWrap from "../Components/CardWrap";

const stateClass = (state: string) => {
  switch (state) {
    case "Ok":
      return "bg-emerald-500/20 text-emerald-700 dark:text-emerald-300";
    case "Risk":
      return "bg-amber-500/20 text-amber-700 dark:text-amber-300";
    case "Problem":
      return "bg-red-500/20 text-red-700 dark:text-red-300";
    default:
      return "bg-slate-200 text-slate-700 dark:bg-slate-500/20 dark:text-slate-400";
  }
};

const SatellitesList = () => {
  const [satellites, setSatellites] = useState<SatelliteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const load = async () => {
      try {
        const data = await SatellitesApiService.list();
        setSatellites(data);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-xl font-semibold text-slate-900 dark:text-indigo-100">Satellites</h1>
        <Button
          onClick={() => navigate("/satellites/new")}
          className="bg-indigo-600 hover:bg-indigo-500 text-white"
        >
          <Plus className="mr-2 h-4 w-4" />
          Create satellite
        </Button>
      </div>

      {loading ? (
        <div className="flex flex-col gap-4">
          {[1, 2, 3].map((i) => (
            <Skeleton
              key={i}
              className="h-16 w-full rounded-xl bg-slate-300 dark:bg-[#626a76]/40"
            />
          ))}
        </div>
      ) : satellites.length === 0 ? (
        <p className="text-slate-700 dark:text-slate-400">No satellites yet. Create one to get started.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-slate-700 dark:border-slate-600 dark:text-slate-400">
                <th className="pb-3 pr-4 font-medium">Name</th>
                <th className="pb-3 pr-4 font-medium">Mission</th>
                <th className="pb-3 pr-4 font-medium">Mode</th>
                <th className="pb-3 pr-4 font-medium">State</th>
                <th className="pb-3 pr-4 font-medium">Link</th>
                <th className="pb-3 font-medium">Last bucket</th>
              </tr>
            </thead>
            <tbody>
              {satellites.map((s) => (
                <tr
                  key={s.id}
                  onClick={() => navigate(`/satellites/${s.id}`)}
                  className="cursor-pointer border-b border-slate-200 hover:bg-slate-50 dark:border-slate-700/50 dark:hover:bg-[#1e2a3a]/50"
                >
                  <td className="flex items-center gap-2 py-3 pr-4">
                    <SatelliteIcon className="h-4 w-4 text-indigo-500 dark:text-indigo-400" />
                    <span className="font-medium text-slate-900 dark:text-indigo-100">{s.name}</span>
                  </td>
                  <td className="py-3 pr-4 text-slate-700 dark:text-slate-300">
                    {s.missionName ?? "—"}
                  </td>
                  <td className="py-3 pr-4 text-slate-700 dark:text-slate-300">{s.mode}</td>
                  <td className="py-3 pr-4">
                    <span className={`rounded px-2 py-0.5 text-xs ${stateClass(s.state)}`}>
                      {s.state}
                    </span>
                  </td>
                  <td className="py-3 pr-4 text-slate-700 dark:text-slate-300">{s.linkStatus}</td>
                  <td className="py-3 text-slate-700 dark:text-slate-400">
                    {s.lastBucketStart
                      ? formatUtcInUserTz(s.lastBucketStart)
                      : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </CardWrap>
  );
};

export default SatellitesList;

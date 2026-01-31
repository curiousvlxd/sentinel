import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { Mission } from "@/types/mission";
import { Plus } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const MissionsList = () => {
  const [missions, setMissions] = useState<Mission[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const load = async () => {
      try {
        const data = await MissionsApiService.list();
        setMissions(data);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-xl font-semibold text-indigo-100">Missions</h1>
        <Button
          onClick={() => navigate("/missions/new")}
          className="bg-indigo-600 hover:bg-indigo-500 text-white"
        >
          <Plus className="mr-2 h-4 w-4" />
          Create mission
        </Button>
      </div>

      {loading ? (
        <div className="flex flex-col gap-4">
          {[1, 2, 3].map((i) => (
            <Skeleton
              key={i}
              className="h-20 w-full rounded-xl bg-[#626a76]/40"
            />
          ))}
        </div>
      ) : missions.length === 0 ? (
        <p className="text-slate-400">No missions yet. Create one to get started.</p>
      ) : (
        <div className="space-y-3">
          {missions.map((m) => (
            <div
              key={m.id}
              onClick={() => navigate(`/missions/${m.id}`)}
              className="flex cursor-pointer items-center justify-between rounded-xl bg-[#1e2a3a] p-4 transition hover:bg-[#263244]"
            >
              <div>
                <h2 className="font-medium text-indigo-100">{m.name}</h2>
                {m.description && (
                  <p className="mt-1 text-sm text-slate-400">{m.description}</p>
                )}
                <p className="mt-1 text-xs text-slate-500">
                  Created {new Date(m.createdAt).toLocaleString()}
                </p>
              </div>
              <span
                className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                  m.isActive ? "bg-emerald-500/20 text-emerald-300" : "bg-slate-500/20 text-slate-400"
                }`}
              >
                {m.isActive ? "Active" : "Inactive"}
              </span>
            </div>
          ))}
        </div>
      )}
    </CardWrap>
  );
};

export default MissionsList;

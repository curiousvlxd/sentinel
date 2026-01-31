import { CommandsApiService } from "@/api/Services/CommandsApiService";
import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { Button } from "@/components/ui/button";
import type { CommandDto } from "@/types/command";
import type { Mission } from "@/types/mission";
import { ArrowLeft } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const STATUSES = ["Queued", "Claimed", "Executed", "Failed", "Canceled", "Expired"];

const MissionCommands = () => {
  const { missionId } = useParams<{ missionId: string }>();
  const navigate = useNavigate();
  const [mission, setMission] = useState<Mission | null>(null);
  const [commands, setCommands] = useState<CommandDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!missionId) return;
    const load = async () => {
      setLoading(true);
      try {
        const [m, list] = await Promise.all([
          MissionsApiService.get(missionId),
          CommandsApiService.listByMission(missionId, statusFilter || undefined),
        ]);
        setMission(m);
        setCommands(list);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [missionId, statusFilter]);

  if (!missionId) {
    return (
      <CardWrap>
        <p className="text-slate-400">Mission ID missing.</p>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate(`/missions/${missionId}`)}
            className="text-slate-400"
          >
            <ArrowLeft className="h-4 w-4 mr-1" />
            Back
          </Button>
          <h1 className="text-xl font-semibold text-indigo-100">
            Command queue
            {mission && ` · ${mission.name}`}
          </h1>
        </div>
      </div>

      <div className="mb-4 flex items-center gap-2">
        <span className="text-sm text-slate-400">Filter by status:</span>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="rounded-md border border-slate-600 bg-[#1e2a3a] px-3 py-2 text-sm text-slate-200"
        >
          <option value="">All</option>
          {STATUSES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="text-slate-400">Loading…</div>
      ) : (
        <div className="overflow-x-auto rounded-xl bg-[#1e2a3a]">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-600 text-slate-400">
                <th className="pb-2 pr-4 font-medium">Type</th>
                <th className="pb-2 pr-4 font-medium">Status</th>
                <th className="pb-2 pr-4 font-medium">Priority</th>
                <th className="pb-2 pr-4 font-medium">TTL</th>
                <th className="pb-2 pr-4 font-medium">Created</th>
                <th className="pb-2 font-medium">Executed</th>
              </tr>
            </thead>
            <tbody>
              {commands.map((c) => (
                <tr
                  key={c.id}
                  className="border-b border-slate-700/50 text-slate-300"
                >
                  <td className="py-2 pr-4">{c.type}</td>
                  <td className="py-2 pr-4">{c.status}</td>
                  <td className="py-2 pr-4">{c.priority}</td>
                  <td className="py-2 pr-4">{c.ttlSec}s</td>
                  <td className="py-2 pr-4">
                    {new Date(c.createdAt).toLocaleString()}
                  </td>
                  <td className="py-2">
                    {c.executedAt
                      ? new Date(c.executedAt).toLocaleString()
                      : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {commands.length === 0 && (
            <p className="p-4 text-slate-500 text-sm">No commands.</p>
          )}
        </div>
      )}
    </CardWrap>
  );
};

export default MissionCommands;

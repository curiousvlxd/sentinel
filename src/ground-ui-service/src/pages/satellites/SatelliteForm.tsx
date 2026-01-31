import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const MODES = ["Autonomous", "Assisted", "Manual"];

const SatelliteForm = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== "new";
  const [name, setName] = useState("");
  const [mode, setMode] = useState("Assisted");
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  useEffect(() => {
    if (!isEdit) return;
    const load = async () => {
      try {
        const s = await SatellitesApiService.get(id!);
        setName(s.name);
        setMode(s.mode || "Assisted");
      } finally {
        setFetching(false);
      }
    };
    load();
  }, [id, isEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setLoading(true);
    try {
      if (isEdit) {
        const sat = await SatellitesApiService.get(id!);
        await SatellitesApiService.update(id!, {
          name: name.trim(),
          status: sat.status,
          mode,
          state: sat.state,
          linkStatus: sat.linkStatus,
        });
        navigate(`/satellites/${id}`);
      } else {
        const created = await SatellitesApiService.create({
          name: name.trim(),
          mode,
        });
        navigate(`/satellites/${created.id}`);
      }
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <CardWrap>
        <div className="flex justify-center py-12">
          <div className="h-10 w-10 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent" />
        </div>
      </CardWrap>
    );
  }

  return (
    <CardWrap>
      <h1 className="mb-6 text-xl font-semibold text-indigo-100">
        {isEdit ? "Edit satellite" : "Create satellite"}
      </h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 max-w-md">
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            Name
          </label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Satellite name"
            required
            className="bg-[#1e2a3a] border-slate-600 text-slate-200"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            Mode
          </label>
          <select
            value={mode}
            onChange={(e) => setMode(e.target.value)}
            className="w-full rounded-md border border-slate-600 bg-[#1e2a3a] px-3 py-2 text-slate-200"
          >
            {MODES.map((m) => (
              <option key={m} value={m}>
                {m}
              </option>
            ))}
          </select>
        </div>
        <div className="flex gap-2">
          <Button
            type="submit"
            disabled={loading || !name.trim()}
            className="bg-indigo-600 hover:bg-indigo-500 text-white"
          >
            {loading ? "Saving…" : "Save"}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(isEdit ? `/satellites/${id}` : "/satellites")}
            className="border-slate-600 text-slate-300"
          >
            Cancel
          </Button>
        </div>
      </form>
    </CardWrap>
  );
};

export default SatelliteForm;

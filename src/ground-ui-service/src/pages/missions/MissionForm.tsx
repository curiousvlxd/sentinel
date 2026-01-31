import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";

const MissionForm = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = id && id !== "new";
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);

  useEffect(() => {
    if (!isEdit) return;
    const load = async () => {
      try {
        const m = await MissionsApiService.get(id!);
        setName(m.name);
        setDescription(m.description ?? "");
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
        const mission = await MissionsApiService.get(id!);
        await MissionsApiService.update(id!, {
          name: name.trim(),
          description: description.trim() || null,
          isActive: mission.isActive,
        });
        navigate(`/missions/${id}`);
      } else {
        const created = await MissionsApiService.create({
          name: name.trim(),
          description: description.trim() || null,
        });
        navigate(`/missions/${created.id}`);
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
        {isEdit ? "Edit mission" : "Create mission"}
      </h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 max-w-md">
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            Name
          </label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Mission name"
            required
            className="bg-[#1e2a3a] border-slate-600 text-slate-200"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            Description (optional)
          </label>
          <Input
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Short description"
            className="bg-[#1e2a3a] border-slate-600 text-slate-200"
          />
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
            onClick={() => navigate(isEdit ? `/missions/${id}` : "/missions")}
            className="border-slate-600 text-slate-300"
          >
            Cancel
          </Button>
        </div>
      </form>
    </CardWrap>
  );
};

export default MissionForm;

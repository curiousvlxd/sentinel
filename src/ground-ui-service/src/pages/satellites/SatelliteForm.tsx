import { SatellitesApiService } from "@/api/Services/SatellitesApiService";
import { FormPageLayout, FormPageSpinner } from "@/components/FormPageLayout";
import { Input } from "@/components/ui/input";
import { useResourceForm } from "@/hooks/useResourceForm";
import type {
  SatelliteDto,
  SatelliteCreateRequest,
  SatelliteUpdateRequest,
} from "@/types/satelliteApi";
import { useNavigate } from "react-router-dom";

const MODES = ["Autonomous", "Assisted", "Manual"];
type SatelliteFormValues = { name: string; mode: string };

const SatelliteForm = () => {
  const navigate = useNavigate();
  const form = useResourceForm<
    SatelliteDto,
    SatelliteFormValues,
    SatelliteCreateRequest,
    SatelliteUpdateRequest
  >({
    basePath: "/satellites",
    resourceLabel: "satellite",
    api: SatellitesApiService,
    defaultValues: { name: "", mode: "Assisted" },
    getInitialValues: (s) => ({ name: s.name, mode: s.mode || "Assisted" }),
    toCreateBody: (v) => ({ name: v.name.trim(), mode: v.mode }),
    toUpdateBody: (s, v) => ({
      name: v.name.trim(),
      status: s.status,
      mode: v.mode,
      state: s.state,
      linkStatus: s.linkStatus,
    }),
    validate: (v) => Boolean(v.name?.trim()),
  });

  if (form.fetching) return <FormPageSpinner />;

  return (
    <FormPageLayout
      title={form.title}
      onCancel={() => navigate(form.cancelHref)}
      onSubmit={form.handleSubmit}
      saving={form.saving}
      submitDisabled={!form.values.name?.trim()}
    >
      <div>
        <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">Name</label>
        <Input
          value={form.values.name}
          onChange={(e) => form.setValue("name", e.target.value)}
          placeholder="Satellite name"
          required
          className={form.inputClass}
        />
      </div>
      <div>
        <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">Mode</label>
        <select
          value={form.values.mode}
          onChange={(e) => form.setValue("mode", e.target.value)}
          className={`w-full rounded-md border px-3 py-2 ${form.inputClass}`}
        >
          {MODES.map((m) => (
            <option key={m} value={m}>{m}</option>
          ))}
        </select>
      </div>
    </FormPageLayout>
  );
};

export default SatelliteForm;

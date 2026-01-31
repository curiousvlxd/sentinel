import { MissionsApiService } from "@/api/Services/MissionsApiService";
import { FormPageLayout, FormPageSpinner } from "@/components/FormPageLayout";
import { Input } from "@/components/ui/input";
import { useResourceForm } from "@/hooks/useResourceForm";
import type { Mission, MissionCreateRequest, MissionUpdateRequest } from "@/types/mission";
import { useNavigate } from "react-router-dom";

type MissionFormValues = { name: string; description: string };

const MissionForm = () => {
  const navigate = useNavigate();
  const form = useResourceForm<Mission, MissionFormValues, MissionCreateRequest, MissionUpdateRequest>({
    basePath: "/missions",
    resourceLabel: "mission",
    api: MissionsApiService,
    defaultValues: { name: "", description: "" },
    getInitialValues: (m) => ({ name: m.name, description: m.description ?? "" }),
    toCreateBody: (v) => ({ name: v.name.trim(), description: v.description?.trim() || null }),
    toUpdateBody: (m, v) => ({
      name: v.name.trim(),
      description: v.description?.trim() || null,
      isActive: m.isActive,
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
          placeholder="Mission name"
          required
          className={form.inputClass}
        />
      </div>
      <div>
        <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">
          Description (optional)
        </label>
        <Input
          value={form.values.description}
          onChange={(e) => form.setValue("description", e.target.value)}
          placeholder="Short description"
          className={form.inputClass}
        />
      </div>
    </FormPageLayout>
  );
};

export default MissionForm;

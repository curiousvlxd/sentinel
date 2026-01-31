import { useEffect, useState, useCallback } from "react";
import { useNavigate, useParams } from "react-router-dom";

const inputClass =
  "border-slate-300 bg-white text-slate-900 dark:border-slate-600 dark:bg-[#1e2a3a] dark:text-slate-200";

export type ResourceFormApi<T, TCreate, TUpdate> = {
  get: (id: string) => Promise<T>;
  create: (body: TCreate) => Promise<{ id: string }>;
  update: (id: string, body: TUpdate) => Promise<unknown>;
};

export function useResourceForm<T, TValues, TCreate, TUpdate>(config: {
  basePath: string;
  resourceLabel: string;
  api: ResourceFormApi<T, TCreate, TUpdate>;
  defaultValues: TValues;
  getInitialValues: (entity: T) => TValues;
  toCreateBody: (values: TValues) => TCreate;
  toUpdateBody: (entity: T, values: TValues) => TUpdate;
  validate: (values: TValues) => boolean;
}) {
  const { basePath, resourceLabel, api, defaultValues, getInitialValues, toCreateBody, toUpdateBody, validate } =
    config;
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = Boolean(id && id !== "new");

  const [values, setValues] = useState<TValues>(defaultValues);
  const [initialEntity, setInitialEntity] = useState<T | null>(null);
  const [fetching, setFetching] = useState(isEdit);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isEdit || !id) return;
    api.get(id).then((entity) => {
      setInitialEntity(entity);
      setValues(getInitialValues(entity));
      setFetching(false);
    });
  }, [id, isEdit]);

  const setValue = useCallback(<K extends keyof TValues>(key: K, value: TValues[K]) => {
    setValues((prev) => ({ ...prev, [key]: value }));
  }, []);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (!validate(values)) return;
      setSaving(true);
      try {
        if (isEdit && id && initialEntity) {
          await api.update(id, toUpdateBody(initialEntity, values));
          navigate(`${basePath}/${id}`);
        } else {
          const created = await api.create(toCreateBody(values));
          navigate(`${basePath}/${created.id}`);
        }
      } finally {
        setSaving(false);
      }
    },
    [values, isEdit, id, initialEntity, basePath, api, toCreateBody, toUpdateBody, validate, navigate]
  );

  return {
    values,
    setValue,
    setValues,
    fetching,
    saving,
    handleSubmit,
    isEdit,
    id,
    cancelHref: isEdit ? `${basePath}/${id}` : basePath,
    title: isEdit ? `Edit ${resourceLabel}` : `Create ${resourceLabel}`,
    inputClass,
  };
}

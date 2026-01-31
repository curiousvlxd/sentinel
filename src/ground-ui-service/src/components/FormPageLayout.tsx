import { Button } from "@/components/ui/button";
import CardWrap from "@/pages/Components/CardWrap";

const spinnerClass =
  "h-10 w-10 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent";
const btnPrimary = "bg-indigo-600 hover:bg-indigo-500 text-white";
const btnOutline = "border-slate-600 text-slate-300";

type FormPageLayoutProps = {
  title: string;
  onCancel: () => void;
  onSubmit: (e: React.FormEvent) => void;
  saving: boolean;
  submitDisabled: boolean;
  children: React.ReactNode;
};

export function FormPageLayout({
  title,
  onCancel,
  onSubmit,
  saving,
  submitDisabled,
  children,
}: FormPageLayoutProps) {
  return (
    <CardWrap>
      <h1 className="mb-6 text-xl font-semibold text-slate-900 dark:text-indigo-100">{title}</h1>
      <form onSubmit={onSubmit} className="flex max-w-md flex-col gap-4">
        {children}
        <div className="flex gap-2">
          <Button type="submit" disabled={saving || submitDisabled} className={btnPrimary}>
            {saving ? "Saving…" : "Save"}
          </Button>
          <Button type="button" variant="outline" onClick={onCancel} className={btnOutline}>
            Cancel
          </Button>
        </div>
      </form>
    </CardWrap>
  );
}

export function FormPageSpinner() {
  return (
    <CardWrap>
      <div className="flex justify-center py-12">
        <div className={spinnerClass} />
      </div>
    </CardWrap>
  );
}

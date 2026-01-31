import type { IsolationForestSummary } from "@/types/isolationForestSummary";
import SummaryCard from "./summaryCard";

interface Props {
  items: IsolationForestSummary[];
}

const SummaryList = ({ items }: Props) => {
  return (
    <div className="flex flex-col items-center gap-4 w-full mt-5">
      {items.map((summary) => (
        <SummaryCard key={summary.bucket_start} summary={summary} />
      ))}
    </div>
  );
};

export default SummaryList;

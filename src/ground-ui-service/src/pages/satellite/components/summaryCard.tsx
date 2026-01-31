import { Badge } from "@/components/ui/badge";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import type { IsolationForestSummary } from "@/types/isolationForestSummary";
import { Brain, Info, ShieldCheck, Sparkles } from "lucide-react";
import { useState } from "react";

interface Props {
  summary: IsolationForestSummary;
}

const SummaryCard = ({ summary }: Props) => {
  const [expanded, setExpanded] = useState(false);

  return (
    <div
      className="w-full max-w-3xl rounded-xl bg-slate-200 dark:bg-[#626a76] p-4 shadow-md hover:shadow-lg transition-all cursor-pointer"
      onClick={() => setExpanded(!expanded)}
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold text-slate-900 dark:text-indigo-100">
            Telemetry Bucket
          </h3>
          <p className="text-xs text-slate-700 dark:text-slate-300">
            {new Date(summary.bucket_start).toLocaleString()}
          </p>

          <div className="mt-2 flex gap-2 text-xs">
            <Badge variant="secondary">
              Score {summary.ml.anomaly_score.toFixed(3)}
            </Badge>
            <Badge variant="secondary">
              Confidence {summary.ml.confidence.toFixed(3)}
            </Badge>
          </div>
        </div>

        <div className="text-slate-600 dark:text-indigo-300 text-sm">{expanded ? "▲" : "▼"}</div>
      </div>

      {expanded && (
        <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
          <div className="rounded-lg bg-slate-100 dark:bg-[#1e2a3a] p-3 border border-slate-300 dark:border-indigo-500/30">
            <div className="flex items-center gap-2 mb-2">
              <Brain className="h-4 w-4 text-indigo-600 dark:text-indigo-400" />
              <h4 className="font-semibold text-slate-800 dark:text-indigo-200">
                Onboard ML Summary
              </h4>

              <TooltipProvider>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <Info className="h-4 w-4 text-slate-600 dark:text-slate-400" />
                  </TooltipTrigger>
                  <TooltipContent className="max-w-xs text-xs">
                    This data is computed onboard and is always available to
                    astronauts, even if the connection to Earth is lost.
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>

              <Badge className="ml-auto bg-emerald-600/20 text-emerald-300">
                ALWAYS AVAILABLE
              </Badge>
            </div>

            <p className="text-xs text-slate-700 dark:text-slate-300 mb-2">
              Model: {summary.ml.model.name} v{summary.ml.model.version}
            </p>

            <div className="space-y-1">
              {Object.entries(summary.ml.per_signal_score).map(
                ([key, value]) => (
                  <div key={key} className="flex justify-between text-xs">
                    <span className="text-slate-700 dark:text-slate-300">{key}</span>
                    <span className="text-slate-900 dark:text-indigo-200 font-mono">
                      {value.toFixed(3)}
                    </span>
                  </div>
                ),
              )}
            </div>

            <div className="mt-3">
              <h5 className="text-xs font-semibold text-slate-800 dark:text-indigo-200 mb-1">
                Top Contributors
              </h5>
              <ul className="list-disc ml-4 text-xs text-slate-700 dark:text-slate-300">
                {summary.ml.top_contributors.map((c) => (
                  <li key={c.key}>
                    {c.key} ({c.weight.toFixed(2)})
                  </li>
                ))}
              </ul>
            </div>
          </div>

          <div className="rounded-lg bg-slate-100 dark:bg-[#1e2a3a] p-3 border border-slate-300 dark:border-indigo-500/20">
            <div className="flex items-center gap-2 mb-2">
              <Sparkles className="h-4 w-4 text-fuchsia-600 dark:text-fuchsia-400" />
              <h4 className="font-semibold text-slate-800 dark:text-indigo-200">
                AI Decision Support
              </h4>

              <TooltipProvider>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <Info className="h-4 w-4 text-slate-600 dark:text-slate-400" />
                  </TooltipTrigger>
                  <TooltipContent className="max-w-xs text-xs">
                    This insight is generated on Earth and may be unavailable
                    during communication outages.
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>

              <Badge className="ml-auto bg-slate-500/20 text-slate-700 dark:text-slate-300">
                EARTH LINK
              </Badge>
            </div>

            {summary.ai_response ? (
              <p className="text-xs text-slate-700 dark:text-slate-300 leading-relaxed">
                {summary.ai_response}
              </p>
            ) : (
              <div className="flex items-center gap-2 text-xs text-slate-600 dark:text-slate-400">
                <ShieldCheck className="h-4 w-4" />
                AI response unavailable
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default SummaryCard;

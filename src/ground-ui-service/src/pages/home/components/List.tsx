import type { Satellite } from "@/types/satellite";
import {
  Activity,
  MapPin,
  Orbit,
  Rocket,
  Satellite as SatelliteIcon,
} from "lucide-react";
import { useNavigate } from "react-router-dom";

interface Props {
  items: Satellite[];
}

const statusColor = (status?: string) => {
  switch (status) {
    case "active":
      return "bg-emerald-500/20 text-emerald-300";
    case "inactive":
      return "bg-yellow-500/20 text-yellow-300";
    case "decommissioned":
      return "bg-red-500/20 text-red-300";
    default:
      return "bg-slate-500/20 text-slate-300";
  }
};

const List = ({ items }: Props) => {
  const navigate = useNavigate();

  return (
    <div className="w-full flex flex-col items-center gap-4">
      {items.map((item) => (
        <div
          key={item.id}
          onClick={() => navigate(`satellites/${item.id}`)}
          className="
            group w-full cursor-pointer
            rounded-xl bg-[#626a76]
            p-4 shadow-md transition-all duration-200
            hover:shadow-xl hover:-translate-y-0.5
          "
        >
          <div className="flex gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-indigo-500/20 text-indigo-300">
              <SatelliteIcon className="h-6 w-6" />
            </div>

            <div className="flex-1">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-indigo-100">
                  {item.name}
                </h3>

                {item.status && (
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusColor(
                      item.status,
                    )}`}
                  >
                    {item.status}
                  </span>
                )}
              </div>

              {item.description && (
                <p className="mt-1 text-sm text-slate-700 dark:text-slate-300">
                  {item.description}
                </p>
              )}

              <div className="mt-3 grid grid-cols-2 gap-x-4 gap-y-1 text-sm text-slate-700 dark:text-slate-300">
                {item.orbitType && (
                  <div className="flex items-center gap-1">
                    <Orbit className="h-4 w-4 text-indigo-300" />
                    <span>{item.orbitType}</span>
                  </div>
                )}

                {item.altitudeKm && (
                  <div className="flex items-center gap-1">
                    <Activity className="h-4 w-4 text-indigo-300" />
                    <span>{item.altitudeKm} km</span>
                  </div>
                )}

                {item.launchDate && (
                  <div className="flex items-center gap-1">
                    <Rocket className="h-4 w-4 text-indigo-300" />
                    <span>
                      {new Date(item.launchDate).toLocaleDateString()}
                    </span>
                  </div>
                )}

                {item.country && (
                  <div className="flex items-center gap-1">
                    <MapPin className="h-4 w-4 text-indigo-300" />
                    <span>{item.country}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default List;

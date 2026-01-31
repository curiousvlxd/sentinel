import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import type { Satellite } from "@/types/satellite";
import type { LucideIcon } from "lucide-react";
import {
  Cpu,
  ExternalLink,
  Globe,
  Orbit,
  Radar,
  Rocket,
  Tag,
} from "lucide-react";

const Section = ({
  icon: Icon,
  title,
  children,
}: {
  icon: LucideIcon;
  title: string;
  children: React.ReactNode;
}) => (
  <div className="rounded-xl bg-[#1e2a3a] p-4 shadow-sm">
    <div className="flex items-center gap-2 mb-3">
      <Icon className="h-5 w-5 text-indigo-400" />
      <h3 className="text-sm font-semibold tracking-wide text-indigo-200">
        {title}
      </h3>
    </div>
    <div className="space-y-2 text-sm text-slate-700 dark:text-slate-300">{children}</div>
  </div>
);

const Row = ({ label, value }: { label: string; value: React.ReactNode }) => (
  <p className="flex justify-between gap-4">
    <span className="text-slate-600 dark:text-slate-400">{label}</span>
    <span className="text-slate-800 dark:text-slate-100 text-right">{value}</span>
  </p>
);

const Metadata = ({ satellite }: { satellite: Satellite }) => {
  return (
    <>
      <h2 className="text-2xl font-bold text-indigo-100 mb-6">
        Satellite Metadata
      </h2>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Section icon={Globe} title="Identity">
          <Row label="ID" value={satellite.id} />
          <Row label="Name" value={satellite.name} />
          {satellite.operator && (
            <Row label="Operator" value={satellite.operator} />
          )}
          {satellite.country && (
            <Row label="Country" value={satellite.country} />
          )}
          {satellite.status && (
            <Row
              label="Status"
              value={<Badge variant="secondary">{satellite.status}</Badge>}
            />
          )}
        </Section>

        <Section icon={Rocket} title="Launch">
          {satellite.launchDate && (
            <Row
              label="Date"
              value={new Date(satellite.launchDate).toLocaleDateString()}
            />
          )}
          {satellite.launchVehicle && (
            <Row label="Vehicle" value={satellite.launchVehicle} />
          )}
          {satellite.launchSite && (
            <Row label="Site" value={satellite.launchSite} />
          )}
        </Section>

        <Section icon={Orbit} title="Orbit">
          {satellite.orbitType && (
            <Row label="Type" value={<Badge>{satellite.orbitType}</Badge>} />
          )}
          {satellite.altitudeKm && (
            <Row label="Altitude" value={`${satellite.altitudeKm} km`} />
          )}
          {satellite.inclinationDeg && (
            <Row label="Inclination" value={`${satellite.inclinationDeg}°`} />
          )}
          {satellite.periodMin && (
            <Row label="Period" value={`${satellite.periodMin} min`} />
          )}
        </Section>

        <Section icon={Radar} title="Payload & Specs">
          {satellite.massKg && (
            <Row label="Mass" value={`${satellite.massKg} kg`} />
          )}
          {satellite.dimensionsM && (
            <Row
              label="Dimensions"
              value={`${satellite.dimensionsM.length} × ${satellite.dimensionsM.width} × ${satellite.dimensionsM.height} m`}
            />
          )}
          {satellite.transponderCount && (
            <Row label="Transponders" value={satellite.transponderCount} />
          )}
          {satellite.resolutionM && (
            <Row label="Resolution" value={`${satellite.resolutionM} m`} />
          )}
        </Section>
      </div>

      {(satellite.tags || satellite.sensors) && (
        <>
          <Separator className="my-6 bg-slate-200 dark:bg-white/10" />

          <div className="space-y-3">
            {satellite.tags && (
              <div className="flex flex-wrap gap-2">
                {satellite.tags.map((tag) => (
                  <Badge key={tag} variant="secondary">
                    <Tag className="h-3 w-3 mr-1" />
                    {tag}
                  </Badge>
                ))}
              </div>
            )}

            {satellite.sensors && (
              <div className="flex flex-wrap gap-2">
                {satellite.sensors.map((sensor) => (
                  <Badge key={sensor} variant="secondary">
                    <Cpu className="h-3 w-3 mr-1" />
                    {sensor}
                  </Badge>
                ))}
              </div>
            )}
          </div>
        </>
      )}

      {satellite.website && (
        <div className="mt-6">
          <a
            href={satellite.website}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-2 rounded-lg bg-indigo-500/20 px-4 py-2 text-sm font-semibold text-indigo-300 hover:bg-indigo-500/30 transition"
          >
            Official Website
            <ExternalLink className="h-4 w-4" />
          </a>
        </div>
      )}
    </>
  );
};

export default Metadata;

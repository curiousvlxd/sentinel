import { ApiService } from "@/api/Services/AssetApiService";
import type { Satellite } from "@/types/satellite";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import CardWrap from "../Components/CardWrap";
import Metadata from "./components/metadata";
import SummaryList from "./components/summaryList";

const SatellitePage = () => {
  const [satellite, setSatellite] = useState<Satellite | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const { id } = useParams<{ id: string }>();

  useEffect(() => {
    if (!id) return;

    const fetchSatellite = async () => {
      setLoading(true);
      const data = await ApiService.getSatelliteById(id);
      setSatellite(data);
      setLoading(false);
    };

    fetchSatellite();
  }, [id]);

  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-500"></div>
      </div>
    );
  }

  if (!satellite) {
    return (
      <div className="flex justify-center items-center h-screen text-slate-300">
        Satellite not found.
      </div>
    );
  }

  return (
    <CardWrap>
      <Metadata satellite={satellite} />
      <SummaryList items={satellite.isolationForestSummary} />
    </CardWrap>
  );
};

export default SatellitePage;

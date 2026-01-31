import { createBrowserRouter } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import Home from "./pages/home/Home";
import MissionCommands from "./pages/missions/MissionCommands";
import MissionDetails from "./pages/missions/MissionDetails";
import MissionForm from "./pages/missions/MissionForm";
import MissionsList from "./pages/missions/MissionsList";
import NotFound from "./pages/notFound/NotFound";
import SatelliteDetails from "./pages/satellite/SatelliteDetails";
import SatelliteForm from "./pages/satellites/SatelliteForm";
import SatellitesList from "./pages/satellites/SatellitesList";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    errorElement: <NotFound />,
    children: [
      {
        index: true,
        element: <Home />,
      },
      {
        path: "missions",
        element: <MissionsList />,
      },
      {
        path: "missions/new",
        element: <MissionForm />,
      },
      {
        path: "missions/:id/edit",
        element: <MissionForm />,
      },
      {
        path: "missions/:id",
        element: <MissionDetails />,
      },
      {
        path: "missions/:missionId/commands",
        element: <MissionCommands />,
      },
      {
        path: "satellites",
        element: <SatellitesList />,
      },
      {
        path: "satellites/new",
        element: <SatelliteForm />,
      },
      {
        path: "satellites/:id/edit",
        element: <SatelliteForm />,
      },
      {
        path: "satellites/:id",
        element: <SatelliteDetails />,
      },
    ],
  },
]);

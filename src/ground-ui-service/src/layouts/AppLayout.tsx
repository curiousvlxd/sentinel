import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { Moon, Sun } from "lucide-react";
import { useTheme } from "@/lib/theme";

const AppLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { theme, toggleTheme } = useTheme();
  const isSatelliteDetails = /^\/satellites\/[^/]+$/.test(location.pathname ?? "");

  const metadata = {
    appName: "SENTINEL",
    team: "Punkrockers",
    year: 2026,
  };

  const goHome = () => navigate("/");

  const tabBase =
    "text-sm font-medium transition-colors " +
    "text-slate-700 hover:text-slate-900 dark:text-slate-300 dark:hover:text-indigo-200";
  const tabActive =
    "text-indigo-600 dark:text-indigo-400 font-semibold";

  return (
    <div className="min-h-screen flex flex-col bg-white text-slate-900 dark:bg-slate-900 dark:text-indigo-100 transition-colors">
      <header className="w-full bg-slate-50 dark:bg-slate-900 border-b border-slate-300 dark:border-slate-700 shadow-sm flex items-center px-4 sm:px-6 py-3 gap-4 shrink-0">
        <div
          className="flex items-center gap-2.5 cursor-pointer group shrink-0"
          onClick={goHome}
        >
          <svg
            className="w-8 h-8 text-indigo-600 dark:text-indigo-400 group-hover:text-indigo-700 dark:group-hover:text-indigo-300 transition-colors"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 2l9 21H3L12 2z"
            />
          </svg>
          <h1 className="text-xl font-bold tracking-wide text-slate-900 dark:text-indigo-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-300 transition-colors hidden sm:block">
            {metadata.appName}
          </h1>
        </div>

        <nav className="flex items-center gap-6 flex-1 min-w-0">
          <NavLink
            to="/"
            className={({ isActive }) =>
              `${tabBase} shrink-0 ${isActive ? tabActive : ""}`
            }
          >
            Home
          </NavLink>
          <NavLink
            to="/missions"
            className={({ isActive }) =>
              `${tabBase} shrink-0 ${isActive ? tabActive : ""}`
            }
          >
            Missions
          </NavLink>
          <NavLink
            to="/satellites"
            className={({ isActive }) =>
              `${tabBase} shrink-0 ${isActive ? tabActive : ""}`
            }
          >
            Satellites
          </NavLink>
        </nav>

        <button
          type="button"
          onClick={toggleTheme}
          className="shrink-0 min-w-10 min-h-10 rounded-xl flex items-center justify-center bg-slate-300 dark:bg-slate-700/80 text-slate-700 dark:text-amber-200 hover:bg-slate-400 dark:hover:bg-slate-600 transition-colors"
          aria-label={theme === "dark" ? "Switch to light theme" : "Switch to dark theme"}
        >
          {theme === "dark" ? (
            <Sun className="w-5 h-5" aria-hidden />
          ) : (
            <Moon className="w-5 h-5" aria-hidden />
          )}
        </button>
      </header>

      <main
        className={`flex-1 w-full mx-auto flex flex-col px-4 py-6 min-h-0 ${isSatelliteDetails ? "max-w-6xl" : "max-w-5xl"}`}
      >
        <Outlet />
      </main>

      <footer className="w-full mt-auto shrink-0 bg-slate-50 dark:bg-slate-900 border-t border-slate-300 dark:border-slate-700 py-4 px-4 sm:px-6 text-sm text-slate-800 dark:text-slate-400">
        © {metadata.year} {metadata.appName}. Team: {metadata.team}.
      </footer>
    </div>
  );
};

export default AppLayout;

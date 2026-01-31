import { NavLink, Outlet, useNavigate } from "react-router-dom";

const AppLayout = () => {
  const navigate = useNavigate();

  const metadata = {
    appName: "SENTINEL",
    command: "punkrockers",
    year: 2026,
  };

  const goHome = () => navigate("/");

  return (
    <div className="min-h-screen flex flex-col bg-slate-800 text-indigo-100">
      <header className="w-full bg-slate-900 shadow-md flex items-center justify-between px-6 py-4">
        <div
          className="flex items-center gap-3 cursor-pointer group"
          onClick={goHome}
        >
          <svg
            className="w-8 h-8 text-indigo-400 group-hover:text-indigo-300 transition-colors"
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

          <h1 className="text-xl font-bold tracking-wide group-hover:text-indigo-300 transition-colors">
            {metadata.appName}
          </h1>
        </div>

        <nav className="flex items-center gap-6 text-sm">
          <NavLink
            to="/"
            className={({ isActive }) =>
              `transition-colors ${
                isActive
                  ? "text-indigo-300 font-semibold"
                  : "text-slate-300 hover:text-indigo-200"
              }`
            }
          >
            Home
          </NavLink>
          <NavLink
            to="/missions"
            className={({ isActive }) =>
              `transition-colors ${
                isActive
                  ? "text-indigo-300 font-semibold"
                  : "text-slate-300 hover:text-indigo-200"
              }`
            }
          >
            Missions
          </NavLink>
          <NavLink
            to="/satellites"
            className={({ isActive }) =>
              `transition-colors ${
                isActive
                  ? "text-indigo-300 font-semibold"
                  : "text-slate-300 hover:text-indigo-200"
              }`
            }
          >
            Satellites
          </NavLink>
        </nav>
      </header>

      <main className="flex-1 w-full max-w-5xl mx-auto flex flex-col px-4 py-6">
        <Outlet />
      </main>

      <footer className="w-full bg-slate-900 py-4 mt-auto px-6 text-sm text-slate-400">
        © {metadata.year} {metadata.appName}. Command: {metadata.command}.
      </footer>
    </div>
  );
};

export default AppLayout;

import express from "express";
import { createProxyMiddleware } from "http-proxy-middleware";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const app = express();

// Aspire injects GROUND_API_URL like http://localhost:5276; from inside Docker that
// is unreachable — use host.docker.internal so the container can reach the host.
function getGroundApiUrl() {
  const raw = process.env.GROUND_API_URL || "http://localhost:5276";
  if (process.env.GROUND_API_URL && raw.includes("localhost")) {
    try {
      const u = new URL(raw);
      if (u.hostname === "localhost" || u.hostname === "127.0.0.1") {
        u.hostname = "host.docker.internal";
        return u.toString();
      }
    } catch (_) {}
  }
  return raw;
}
const groundApiUrl = getGroundApiUrl();

app.use(
  "/api",
  createProxyMiddleware({
    target: groundApiUrl,
    changeOrigin: true,
  })
);

app.use(express.static(path.join(__dirname, "dist")));
app.get("*", (req, res, next) => {
  if (req.path.startsWith("/api")) return next();
  res.sendFile(path.join(__dirname, "dist", "index.html"));
});

const port = process.env.PORT || 80;
app.listen(port);

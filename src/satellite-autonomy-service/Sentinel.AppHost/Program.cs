var builder = DistributedApplication.CreateBuilder(args);

var appHostDir = builder.AppHostDirectory;
var repoRoot = Path.Combine(appHostDir, "..", "..", "..");

var satelliteDb = builder.AddContainer("satellite-db", "timescale/timescaledb", "latest-pg16")
    .WithEndpoint(port: 5433, targetPort: 5432, name: "pg")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "satellite")
    .WithBindMount(Path.Combine(repoRoot, "infra", "sql", "satellite-db"), "/docker-entrypoint-initdb.d");

var groundDb = builder.AddContainer("ground-db", "postgres", "16")
    .WithEndpoint(port: 5434, targetPort: 5432, name: "pg")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "ground")
    .WithBindMount(Path.Combine(repoRoot, "infra", "sql", "ground-db"), "/docker-entrypoint-initdb.d");

var onboardAi = builder.AddDockerfile("onboard-ai", Path.Combine(repoRoot, "src", "onboard-ai-service"))
    .WithEndpoint(port: 8000, targetPort: 8000, name: "http");

var satellite = builder.AddProject("satellite", Path.Combine(appHostDir, "..", "Sentinel.Satellite.Service", "Sentinel.Satellite.Service.csproj"))
    .WithEnvironment("Timescale__ConnectionString", "Host=localhost;Port=5433;Database=satellite;Username=postgres;Password=postgres");

var ground = builder.AddProject("ground", Path.Combine(appHostDir, "..", "Sentinel.Ground.Api", "Sentinel.Ground.Api.csproj"))
    .WithEnvironment("Ground__TimescaleConnectionString", "Host=localhost;Port=5433;Database=satellite;Username=postgres;Password=postgres")
    .WithEnvironment("Ground__GroundDbConnectionString", "Host=localhost;Port=5434;Database=ground;Username=postgres;Password=postgres")
    .WithEnvironment("Ground__MlServiceBaseUrl", "http://localhost:8000")
    .WithEnvironment("Ground__AnomalyThreshold", "0.7");

await builder.Build().RunAsync();

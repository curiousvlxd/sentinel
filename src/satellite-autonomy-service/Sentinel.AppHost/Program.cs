using Scalar.Aspire;
using Sentinel.AppHost;
using Sentinel.AppHost.Configuration;
using Sentinel.AppHost.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var options = new SentinelHostOptions();
new SentinelHostOptionsSetup(builder.Configuration).Configure(options);

var appHostDir = builder.AppHostDirectory;
var repoRoot = Path.GetFullPath(Path.Combine(appHostDir, "..", "..", ".."));

var volumesDir = Path.Combine(repoRoot, options.Volumes.Folder);
var groundDbVolumePath = Path.Combine(volumesDir, options.GroundDb.VolumeSubdir);
var initSatelliteDbPath = Path.Combine(repoRoot, options.SatelliteDb.InfraSqlPath.Replace('/', Path.DirectorySeparatorChar));

Directory.CreateDirectory(groundDbVolumePath);

var postgresPassword = builder.AddParameter("postgres-password");
var groundPostgres = builder.AddPostgres("ground-db")
    .WithPassword(postgresPassword)
    .WithDataBindMount(groundDbVolumePath, isReadOnly: false);
var groundDb = groundPostgres.AddDatabase(options.GroundDb.Name);

var onboardAiPath = Path.Combine(repoRoot, options.OnboardAi.DockerfilePath.Replace('/', Path.DirectorySeparatorChar));

var onboardAiNamePrefix = "onboard-ai";
var onboardAiPortBase = options.OnboardAi.Port;
var instanceCount = options.MaxSatelliteInstances > 0
    ? Math.Min(options.MaxSatelliteInstances, options.SatelliteInstances.Count)
    : options.SatelliteInstances.Count;
var activeInstances = options.SatelliteInstances.Take(instanceCount).ToList();
var seedSatelliteIds = string.Join(";", activeInstances.Select(x => x.Id));

IResourceBuilder<ProjectResource>? firstSatelliteProject = null;

var groundWithSeed = builder.AddProject<Projects.Sentinel_Ground_Api>("ground-api")
    .WaitFor(groundDb)
    .WithReference(groundDb, connectionName: "ground")
    .WithEnvironment(ConfigKeys.GroundMigrateOnStartup, options.Ground.MigrateOnStartup)
    .WithEnvironment(ConfigKeys.GroundAnomalyThreshold, options.Ground.AnomalyThreshold)
    .WithEnvironment(ConfigKeys.SeedMissionId, options.Seed.MissionId)
    .WithEnvironment(ConfigKeys.SeedSatelliteIds, seedSatelliteIds);

var scalar = builder.AddScalarApiReference();
scalar.WithApiReference(groundWithSeed);

for (var i = 0; i < instanceCount; i++)
{
    var instance = activeInstances[i];
    var groupName = $"group-{instance.Name}";
    var group = builder.AddGroup(groupName);

    var satelliteDbVolumePath = Path.Combine(volumesDir, $"{options.SatelliteDb.VolumeSubdir}-{instance.Name}");
    Directory.CreateDirectory(satelliteDbVolumePath);
    var satellitePostgres = builder.AddPostgres($"satellite-db-{instance.Name}")
        .WithImage(options.SatelliteDb.Image, options.SatelliteDb.Tag)
        .WithPassword(postgresPassword)
        .WithDataBindMount(satelliteDbVolumePath, isReadOnly: false)
        .WithInitFiles(initSatelliteDbPath)
        .WithEnvironment("POSTGRES_DB", options.SatelliteDb.Name)
        .WithParentRelationship(group);
    var satelliteDb = satellitePostgres.AddDatabase($"satellite-{instance.Name}", options.SatelliteDb.Name);

    var onboardAiResourceName = $"{onboardAiNamePrefix}-{instance.Name}";
    var onboardAiPort = onboardAiPortBase + i;
    var onboardAi = builder.AddDockerfile(onboardAiResourceName, onboardAiPath)
        .WithEndpoint(port: onboardAiPort, targetPort: options.OnboardAi.Port, name: "http")
        .WithParentRelationship(group);

    if (i == 0)
        groundWithSeed = groundWithSeed.WithEnvironment("OnboardAi__Url", onboardAi.GetEndpoint("http"));

    var satelliteProject = builder.AddProject<Projects.Sentinel_Satellite_Service>(instance.Name)
        .WaitFor(satelliteDb)
        .WithReference(groundDb)
        .WithReference(satelliteDb, connectionName: "satellite")
        .WithEnvironment(ConfigKeys.GroundAnomalyThreshold, options.Ground.AnomalyThreshold)
        .WithEnvironment(ConfigKeys.SatelliteId, instance.Id)
        .WithEnvironment(ConfigKeys.SatelliteInstanceName, instance.Name)
        .WithEnvironment(ConfigKeys.SatelliteOnboardAiServiceName, onboardAiResourceName)
        .WithEnvironment(ConfigKeys.SimulatorMissionId, options.Seed.MissionId)
        .WithEnvironment(ConfigKeys.SimulatorSatelliteId, instance.Id)
        .WithEnvironment(context =>
        {
            context.EnvironmentVariables[$"Services__{onboardAiResourceName}__http__0"] = onboardAi.GetEndpoint("http");
            context.EnvironmentVariables["Services__ground-api__http__0"] = groundWithSeed.GetEndpoint("http");
        })
        .WithParentRelationship(group);
    if (i == 0) firstSatelliteProject = satelliteProject;

    scalar.WithApiReference(satelliteProject);
}

var groundUiPath = Path.Combine(repoRoot, "src", "ground-ui-service");
if (Directory.Exists(groundUiPath))
{
    builder.AddDockerfile("ground-ui", groundUiPath)
        .WithHttpEndpoint(port: 5180, targetPort: 80, name: "http")
        .WithEnvironment("GROUND_API_URL", groundWithSeed.GetEndpoint("http"));
}

await builder.Build().RunAsync().ConfigureAwait(false);

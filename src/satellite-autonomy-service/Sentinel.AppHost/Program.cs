using Scalar.Aspire;
using Sentinel.AppHost.Configuration;
using Sentinel.AppHost.Constants;
using Sentinel.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
var options = new SentinelHostOptions();
new SentinelHostOptionsSetup(builder.Configuration).Configure(options);

var appHostDir = builder.AppHostDirectory;
var repoRoot = Path.GetFullPath(Path.Combine(appHostDir, "..", "..", ".."));

var volumesDir = Path.Combine(repoRoot, options.Volumes.Folder);
var groundDbVolumePath = Path.Combine(volumesDir, options.GroundDb.VolumeSubdir);
var initSatelliteDbPath = Path.Combine(repoRoot, options.SatelliteDb.InfraSqlPath.Replace('/', Path.DirectorySeparatorChar));
var initGroundDbPath = Path.Combine(repoRoot, options.GroundDb.InfraSqlPath.Replace('/', Path.DirectorySeparatorChar));

Directory.CreateDirectory(groundDbVolumePath);

var groundPostgres = builder.AddPostgres("ground-db")
    .WithDataBindMount(groundDbVolumePath, isReadOnly: false)
    .WithInitFiles(initGroundDbPath);
var groundDb = groundPostgres.AddDatabase(options.GroundDb.Name);

var onboardAiPath = Path.Combine(repoRoot, options.OnboardAi.DockerfilePath.Replace('/', Path.DirectorySeparatorChar));

var ground = builder.AddProject<Projects.Sentinel_Ground_Api>("ground-api")
    .WithReference(groundDb)
    .WithEnvironment(ConfigKeys.GroundMigrateOnStartup, options.Ground.MigrateOnStartup)
    .WithEnvironment(ConfigKeys.GroundAnomalyThreshold, options.Ground.AnomalyThreshold)
    .WithEnvironment(ConfigKeys.SeedMissionId, options.Seed.MissionId)
    .WithEnvironment(ConfigKeys.SeedSatelliteIds, options.Seed.SatelliteIds);

var scalar = builder.AddScalarApiReference();
scalar.WithApiReference(ground);

var onboardAiNamePrefix = "onboard-ai";
var onboardAiPortBase = options.OnboardAi.Port;
var satelliteDbPortBase = options.SatelliteDb.Port;

for (var i = 0; i < options.SatelliteInstances.Count; i++)
{
    var instance = options.SatelliteInstances[i];
    var groupName = $"group-{instance.Name}";
    var group = builder.AddGroup(groupName);

    var satelliteDbVolumePath = Path.Combine(volumesDir, $"{options.SatelliteDb.VolumeSubdir}-{instance.Name}");
    Directory.CreateDirectory(satelliteDbVolumePath);
    var satelliteDbPort = satelliteDbPortBase + i;
    var satellitePostgres = builder.AddPostgres($"satellite-db-{instance.Name}")
        .WithImage(options.SatelliteDb.Image, options.SatelliteDb.Tag)
        .WithEndpoint(port: satelliteDbPort, targetPort: 5432, name: "pg")
        .WithDataBindMount(satelliteDbVolumePath, isReadOnly: false)
        .WithInitFiles(initSatelliteDbPath)
        .WithEnvironment("POSTGRES_USER", options.Postgres.User)
        .WithEnvironment("POSTGRES_PASSWORD", options.Postgres.Password)
        .WithEnvironment("POSTGRES_DB", options.SatelliteDb.Name)
        .WithParentRelationship(group);
    var satelliteDb = satellitePostgres.AddDatabase($"satellite-{instance.Name}", options.SatelliteDb.Name);

    var onboardAiResourceName = $"{onboardAiNamePrefix}-{instance.Name}";
    var onboardAiPort = onboardAiPortBase + i;
    var onboardAi = builder.AddDockerfile(onboardAiResourceName, onboardAiPath)
        .WithEndpoint(port: onboardAiPort, targetPort: options.OnboardAi.Port, name: "http")
        .WithParentRelationship(group);

    var satelliteProject = builder.AddProject<Projects.Sentinel_Satellite_Service>(instance.Name)
        .WithReference(groundDb)
        .WithReference(satelliteDb, connectionName: "satellite")
        .WithEnvironment(ConfigKeys.GroundAnomalyThreshold, options.Ground.AnomalyThreshold)
        .WithEnvironment(ConfigKeys.SatelliteId, instance.Id)
        .WithEnvironment(ConfigKeys.SatelliteInstanceName, instance.Name)
        .WithEnvironment(ConfigKeys.SatelliteOnboardAiServiceName, onboardAiResourceName)
        .WithEnvironment(ConfigKeys.SimulatorMissionId, options.Seed.MissionId)
        .WithEnvironment(context =>
        {
            context.EnvironmentVariables[$"Services__{onboardAiResourceName}__http__0"] = onboardAi.GetEndpoint("http");
            context.EnvironmentVariables["Services__ground-api__http__0"] = ground.GetEndpoint("http");
        })
        .WithParentRelationship(group);
    scalar.WithApiReference(satelliteProject);
}

await builder.Build().RunAsync();

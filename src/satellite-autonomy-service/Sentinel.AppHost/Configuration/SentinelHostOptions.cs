namespace Sentinel.AppHost.Configuration;

public sealed class SentinelHostOptions
{
    public const string SectionName = "Sentinel";

    public SentinelHostOptions() { }

    public PostgresOptions Postgres { get; set; } = null!;
    public SatelliteDbOptions SatelliteDb { get; set; } = null!;
    public GroundDbOptions GroundDb { get; set; } = null!;
    public VolumesOptions Volumes { get; set; } = null!;
    public ContainerOptions Container { get; set; } = null!;
    public OnboardAiOptions OnboardAi { get; set; } = null!;
    public GroundOptions Ground { get; set; } = null!;
    public SeedOptions Seed { get; set; } = null!;
    public List<SatelliteInstanceOptions> SatelliteInstances { get; set; } = new();
    public int MaxSatelliteInstances { get; set; }

    public sealed class PostgresOptions
    {
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public sealed class SatelliteDbOptions
    {
        public string Name { get; set; } = "";
        public int Port { get; set; }
        public string Image { get; set; } = "";
        public string Tag { get; set; } = "";
        public string InfraSqlPath { get; set; } = "";
        public string VolumeSubdir { get; set; } = "";
    }

    public sealed class GroundDbOptions
    {
        public string Name { get; set; } = "";
        public string VolumeSubdir { get; set; } = "";
    }

    public sealed class VolumesOptions
    {
        public string Folder { get; set; } = "";
    }

    public sealed class ContainerOptions
    {
        public string InitDbPath { get; set; } = "";
        public string PgDataPath { get; set; } = "";
    }

    public sealed class OnboardAiOptions
    {
        public int Port { get; set; }
        public string DockerfilePath { get; set; } = "";
    }

    public sealed class GroundOptions
    {
        public string AnomalyThreshold { get; set; } = "";
        public string MigrateOnStartup { get; set; } = "";
    }

    public sealed class SeedOptions
    {
        public required string MissionId { get; set; }
        public required string SatelliteIds { get; set; }
    }

    public sealed class SatelliteInstanceOptions
    {
        public required string Name { get; set; }
        public required string Id { get; set; }
    }
}

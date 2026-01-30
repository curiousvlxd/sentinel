namespace Sentinel.Infrastructure.Persistence.Options;

public sealed class GroundDatabaseOptions
{
    public const string SectionName = "Ground:Database";
    public string ConnectionString { get; set; } = string.Empty;
    public bool MigrateOnStartup { get; init; }
}

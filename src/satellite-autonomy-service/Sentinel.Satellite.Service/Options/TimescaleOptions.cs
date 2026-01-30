namespace Sentinel.Satellite.Service.Options;

public sealed class TimescaleOptions
{
    public const string SectionName = "Timescale";
    public string ConnectionString { get; set; } = string.Empty;
}

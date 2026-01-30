namespace Sentinel.Satellite.Service.Options;

public sealed class SatelliteHealthCheckOptions
{
    public const string SectionName = "SatelliteHealthCheck";
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
}

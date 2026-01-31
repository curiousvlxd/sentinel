namespace Sentinel.Core.Contracts.Telemetry;

public sealed record TelemetryBucketRequest
{
    public string SchemaVersion { get; set; } = "v1";

    public Guid SatelliteId { get; set; }

    public string BucketStart { get; set; } = string.Empty;

    public int BucketSec { get; set; } = 60;

    public Dictionary<string, SignalAggregateContract> Signals { get; set; } = new();
}

public sealed record SignalAggregateContract
{
    public double Mean { get; set; }

    public double Min { get; set; }

    public double Max { get; set; }

    public double Std { get; set; }

    public double Slope { get; set; }

    public double P95 { get; set; }

    public double MissingRate { get; set; }
}

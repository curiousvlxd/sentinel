namespace Sentinel.Core.Entities;

public sealed class MlHealthResult
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public Satellite? Satellite { get; set; }

    public DateTimeOffset BucketStart { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;

    public double AnomalyScore { get; set; }

    public double Confidence { get; set; }

    public string PerSignalScore { get; set; } = "{}";

    public string TopContributors { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
}

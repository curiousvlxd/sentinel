namespace Sentinel.Ground.Application.Features.Satellites.Common.Contracts;

public sealed record SatelliteMlResultsResponse
{
    public IReadOnlyList<SatelliteMlResultItem> Response { get; set; } = [];

    public static SatelliteMlResultsResponse Create(IReadOnlyList<SatelliteMlResultItem> response) =>
        new() { Response = response };
}

public sealed record SatelliteMlResultItem
{
    public Guid Id { get; set; }

    public Guid SatelliteId { get; set; }

    public string BucketStart { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;

    public double AnomalyScore { get; set; }

    public double Confidence { get; set; }

    public IReadOnlyDictionary<string, double> PerSignalScore { get; set; } = new Dictionary<string, double>();

    public IReadOnlyList<MlTopContributorItem> TopContributors { get; set; } = new List<MlTopContributorItem>();

    public string CreatedAt { get; set; } = string.Empty;
}

public sealed record MlTopContributorItem
{
    public string Key { get; set; } = string.Empty;

    public double Weight { get; set; }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteMlResults;

[Mapper]
public static partial class GetSatelliteMlResultsMapper
{
    [MapProperty(nameof(MlHealthResult.BucketStart), nameof(SatelliteMlResultItem.BucketStart), StringFormat = "O")]
    [MapProperty(nameof(MlHealthResult.CreatedAt), nameof(SatelliteMlResultItem.CreatedAt), StringFormat = "O")]
    [MapProperty(nameof(MlHealthResult.PerSignalScore), nameof(SatelliteMlResultItem.PerSignalScore), Use = nameof(ParsePerSignalScore))]
    [MapProperty(nameof(MlHealthResult.TopContributors), nameof(SatelliteMlResultItem.TopContributors), Use = nameof(ParseTopContributors))]
    public static partial SatelliteMlResultItem Map(this MlHealthResult source);

    extension(IQueryable<MlHealthResult> query)
    {
        public MlResultsProjection ToResponse() => new(query);

        public async Task<List<SatelliteMlResultItem>> ToResponseAsync(CancellationToken cancellationToken = default)
        {
            var rows = await query
                .Select(m => new MlHealthResultRow
                {
                    Id = m.Id,
                    SatelliteId = m.SatelliteId,
                    BucketStart = m.BucketStart.ToString("O"),
                    ModelName = m.ModelName,
                    ModelVersion = m.ModelVersion,
                    AnomalyScore = m.AnomalyScore,
                    Confidence = m.Confidence,
                    PerSignalScore = m.PerSignalScore,
                    TopContributors = m.TopContributors,
                    CreatedAt = m.CreatedAt.ToString("O"),
                })
                .ToListAsync(cancellationToken);
            return rows.Select(MapRowToItem).ToList();
        }
    }

    public readonly struct MlResultsProjection(IQueryable<MlHealthResult> query)
    {
        public Task<List<SatelliteMlResultItem>> ToListAsync(CancellationToken cancellationToken = default) =>
            query.ToResponseAsync(cancellationToken);
    }

    private static SatelliteMlResultItem MapRowToItem(MlHealthResultRow row) => new()
    {
        Id = row.Id,
        SatelliteId = row.SatelliteId,
        BucketStart = row.BucketStart,
        ModelName = row.ModelName,
        ModelVersion = row.ModelVersion,
        AnomalyScore = row.AnomalyScore,
        Confidence = row.Confidence,
        PerSignalScore = ParsePerSignalScore(row.PerSignalScore),
        TopContributors = ParseTopContributors(row.TopContributors),
        CreatedAt = row.CreatedAt,
    };

    private sealed record MlHealthResultRow
    {
        public Guid Id { get; init; }

        public Guid SatelliteId { get; init; }

        public string BucketStart { get; init; } = string.Empty;

        public string ModelName { get; init; } = string.Empty;

        public string ModelVersion { get; init; } = string.Empty;

        public double AnomalyScore { get; init; }

        public double Confidence { get; init; }

        public string PerSignalScore { get; init; } = "{}";

        public string TopContributors { get; init; } = "[]";

        public string CreatedAt { get; init; } = string.Empty;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static IReadOnlyDictionary<string, double> ParsePerSignalScore(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, double>();
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, double>>(json, _jsonOptions);
            return dict ?? new Dictionary<string, double>();
        }
        catch
        {
            return new Dictionary<string, double>();
        }
    }

    private static IReadOnlyList<MlTopContributorItem> ParseTopContributors(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<MlTopContributorItem>();
        try
        {
            var list = JsonSerializer.Deserialize<List<MlTopContributorItem>>(json, _jsonOptions);
            return list ?? [];
        }
        catch
        {
            return new List<MlTopContributorItem>();
        }
    }
}

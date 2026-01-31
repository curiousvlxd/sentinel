using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteMlResults;

public sealed class GetSatelliteMlResultsQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatelliteMlResultsQuery, Result<SatelliteMlResultsResponse>>
{
    public async Task<Result<SatelliteMlResultsResponse>> Handle(GetSatelliteMlResultsQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = request.From ?? DateTimeOffset.UtcNow.AddDays(-1);
        var toUtc = request.To ?? DateTimeOffset.UtcNow;
        var response = await context.MlHealthResults
            .Where(m => m.SatelliteId == request.SatelliteId && m.BucketStart >= fromUtc && m.BucketStart <= toUtc)
            .OrderByDescending(m => m.BucketStart)
            .ToResponse()
            .ToListAsync(cancellationToken);

        return SatelliteMlResultsResponse.Create(response);
    }
}

using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Features.Decisions.GetDecisionById;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteDecisions;

public sealed class GetSatelliteDecisionsQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatelliteDecisionsQuery, Result<IReadOnlyList<DecisionResponse>>>
{
    public async Task<Result<IReadOnlyList<DecisionResponse>>> Handle(GetSatelliteDecisionsQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = request.From ?? DateTimeOffset.UtcNow.AddDays(-1);
        var toUtc = request.To ?? DateTimeOffset.UtcNow;
        var list = await context.Decisions
            .Where(d => d.SatelliteId == request.SatelliteId && d.BucketStart >= fromUtc && d.BucketStart <= toUtc)
            .OrderByDescending(d => d.BucketStart)
            .Take(request.Limit)
            .ToResponse()
            .ToListAsync(cancellationToken);
        return list;
    }
}

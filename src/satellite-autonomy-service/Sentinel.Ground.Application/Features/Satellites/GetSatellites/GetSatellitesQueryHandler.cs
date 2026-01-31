using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatellites;

public sealed class GetSatellitesQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatellitesQuery, Result<IReadOnlyList<SatelliteResponse>>>
{
    public async Task<Result<IReadOnlyList<SatelliteResponse>>> Handle(GetSatellitesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Satellites.Include(s => s.Mission).AsQueryable();

        if (request.MissionId.HasValue)
            query = query.Where(s => s.MissionId == request.MissionId.Value);

        var list = await query.OrderBy(s => s.Name).ToResponse().ToListAsync(cancellationToken);
        return list;
    }
}

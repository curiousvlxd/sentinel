using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Features.Missions.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.GetMissions;

public sealed class GetMissionsQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetMissionsQuery, Result<IReadOnlyList<MissionResponse>>>
{
    public async Task<Result<IReadOnlyList<MissionResponse>>> Handle(GetMissionsQuery request, CancellationToken cancellationToken)
    {
        var list = await MissionMapper.ToResponse(context.Missions
                .OrderBy(m => m.Name))
            .ToListAsync(cancellationToken);
        return list;
    }
}

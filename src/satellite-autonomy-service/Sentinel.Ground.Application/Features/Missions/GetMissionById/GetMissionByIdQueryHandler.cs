using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Features.Missions.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.GetMissionById;

public sealed class GetMissionByIdQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetMissionByIdQuery, Result<MissionResponse>>
{
    public async Task<Result<MissionResponse>> Handle(GetMissionByIdQuery request, CancellationToken cancellationToken)
    {
        var mission = await MissionMapper.ToResponse(context.Missions
                .Where(x => x.Id == request.MissionId))
            .FirstOrDefaultAsync(cancellationToken);

        return mission is null ? Error.NotFound("Mission not found.") : mission;
    }
}

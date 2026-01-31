using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Features.Missions.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.UpdateMission;

public sealed class UpdateMissionCommandHandler(IGroundDbContext context)
    : ICommandHandler<UpdateMissionCommand, Result<MissionResponse>>
{
    public async Task<Result<MissionResponse>> Handle(UpdateMissionCommand request, CancellationToken cancellationToken)
    {
        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission is null)
            return Error.NotFound("Mission not found.");

        mission.Update(request.Request.Name, request.Request.Description, request.Request.IsActive);
        await context.SaveChangesAsync(cancellationToken);
        return MissionMapper.ToResponse(mission);
    }
}

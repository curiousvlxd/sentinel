using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Features.Missions.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.CreateMission;

public sealed class CreateMissionCommandHandler(IGroundDbContext context)
    : ICommandHandler<CreateMissionCommand, Result<MissionResponse>>
{
    public async Task<Result<MissionResponse>> Handle(CreateMissionCommand request, CancellationToken cancellationToken)
    {
        var mission = Mission.Create(request.Request.Name, request.Request.Description);
        context.Add(mission);
        await context.SaveChangesAsync(cancellationToken);
        return MissionMapper.ToResponse(mission);
    }
}

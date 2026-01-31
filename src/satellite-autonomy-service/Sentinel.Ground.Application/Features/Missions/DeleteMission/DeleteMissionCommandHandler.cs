using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.DeleteMission;

public sealed class DeleteMissionCommandHandler(IGroundDbContext context)
    : ICommandHandler<DeleteMissionCommand, Result>
{
    public async Task<Result> Handle(DeleteMissionCommand request, CancellationToken cancellationToken)
    {
        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission is null)
            return Error.NotFound("Mission not found.");

        var satellites = await context.Satellites.Where(s => s.MissionId == request.MissionId).ToListAsync(cancellationToken);
        foreach (var satellite in satellites)
            satellite.MissionId = null;

        context.RemoveMission(mission);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

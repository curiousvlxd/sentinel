using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.AttachSatellite;

public sealed class AttachSatelliteCommandHandler(IGroundDbContext context)
    : ICommandHandler<AttachSatelliteCommand, Result>
{
    public async Task<Result> Handle(AttachSatelliteCommand request, CancellationToken cancellationToken)
    {
        var missionExists = await context.Missions.AnyAsync(m => m.Id == request.MissionId, cancellationToken);

        if (!missionExists)
            return Error.NotFound("Mission not found.");

        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == request.SatelliteId, cancellationToken);

        if (satellite is null)
            return Error.NotFound("Satellite not found.");

        satellite.MissionId = request.MissionId;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

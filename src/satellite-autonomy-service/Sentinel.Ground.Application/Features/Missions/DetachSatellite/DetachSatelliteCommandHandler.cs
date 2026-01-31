using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.DetachSatellite;

public sealed class DetachSatelliteCommandHandler(IGroundDbContext context)
    : ICommandHandler<DetachSatelliteCommand, Result>
{
    public async Task<Result> Handle(DetachSatelliteCommand request, CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites
            .FirstOrDefaultAsync(s => s.Id == request.SatelliteId && s.MissionId == request.MissionId, cancellationToken);

        if (satellite is null)
            return Error.NotFound("Satellite not in mission.");

        satellite.MissionId = null;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

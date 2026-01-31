using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.DeleteSatellite;

public sealed class DeleteSatelliteCommandHandler(IGroundDbContext context)
    : ICommandHandler<DeleteSatelliteCommand, Result>
{
    public async Task<Result> Handle(DeleteSatelliteCommand request, CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (satellite is null)
            return Error.NotFound("Satellite not found.");

        context.RemoveSatellite(satellite);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

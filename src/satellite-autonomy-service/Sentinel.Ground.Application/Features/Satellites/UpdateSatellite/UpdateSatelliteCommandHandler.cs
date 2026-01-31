using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.UpdateSatellite;

public sealed class UpdateSatelliteCommandHandler(IGroundDbContext context)
    : ICommandHandler<UpdateSatelliteCommand, Result<SatelliteResponse>>
{
    public async Task<Result<SatelliteResponse>> Handle(UpdateSatelliteCommand request, CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.Include(s => s.Mission)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (satellite is null)
            return Error.NotFound("Satellite not found.");

        satellite.Update(
            request.Request.Name,
            request.Request.Status,
            request.Request.Mode,
            request.Request.State,
            request.Request.LinkStatus);

        await context.SaveChangesAsync(cancellationToken);
        return CreateSatelliteMapper.ToResponse(satellite, satellite.Mission?.Name);
    }
}

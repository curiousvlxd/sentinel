using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteById;

public sealed class GetSatelliteByIdQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatelliteByIdQuery, Result<SatelliteResponse>>
{
    public async Task<Result<SatelliteResponse>> Handle(GetSatelliteByIdQuery request, CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.Include(x => x.Mission)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return satellite is null ? Error.NotFound("Satellite not found.") : CreateSatelliteMapper.ToResponse(satellite, satellite.Mission?.Name);
    }
}

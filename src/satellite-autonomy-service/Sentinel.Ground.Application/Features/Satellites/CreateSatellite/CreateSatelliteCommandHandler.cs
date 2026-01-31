using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.CreateSatellite;

public sealed class CreateSatelliteCommandHandler(IGroundDbContext context)
    : ICommandHandler<CreateSatelliteCommand, Result<SatelliteResponse>>
{
    public async Task<Result<SatelliteResponse>> Handle(CreateSatelliteCommand request, CancellationToken cancellationToken)
    {
        var satellite = Satellite.Create(request.Request.Name, null, request.Request.Mode);
        context.Add(satellite);
        await context.SaveChangesAsync(cancellationToken);
        return CreateSatelliteMapper.ToResponse(satellite, null);
    }
}

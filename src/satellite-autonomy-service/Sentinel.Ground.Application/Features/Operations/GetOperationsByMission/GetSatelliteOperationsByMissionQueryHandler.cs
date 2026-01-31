using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Features.Operations.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.GetOperationsByMission;

public sealed class GetSatelliteOperationsByMissionQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatelliteOperationsByMissionQuery, Result<IReadOnlyList<SatelliteOperationResponse>>>
{
    public async Task<Result<IReadOnlyList<SatelliteOperationResponse>>> Handle(GetSatelliteOperationsByMissionQuery request, CancellationToken cancellationToken)
    {
        var query = context.SatelliteOperations.Where(o => o.MissionId == request.MissionId);

        if (request.Status is { } status)
            query = query.Where(o => o.Status == status);

        var operations = await query.OrderByDescending(o => o.CreatedAt).ToResponse().ToListAsync(cancellationToken);
        return operations;
    }
}

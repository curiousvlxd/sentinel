using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Features.Operations.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.GetOperationById;

public sealed class GetSatelliteOperationByIdQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetSatelliteOperationByIdQuery, Result<SatelliteOperationResponse>>
{
    public async Task<Result<SatelliteOperationResponse>> Handle(GetSatelliteOperationByIdQuery request, CancellationToken cancellationToken)
    {
        var operation = await context.SatelliteOperations.FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);
        return operation is null ? Error.NotFound("Satellite operation not found.") : CreateOperationMapper.ToResponse(operation);
    }
}

using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Features.Operations.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.PullOperations;

public sealed class PullSatelliteOperationsCommandHandler(IGroundDbContext context)
    : ICommandHandler<PullSatelliteOperationsCommand, Result<IReadOnlyList<SatelliteOperationResponse>>>
{
    public async Task<Result<IReadOnlyList<SatelliteOperationResponse>>> Handle(PullSatelliteOperationsCommand request, CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow;
        var queuedOperations = await context.SatelliteOperations
            .Where(o => o.SatelliteId == request.SatelliteId && o.Status == SatelliteOperationStatus.Queued && o.CreatedAt.AddSeconds(o.TtlSec) >= cutoff)
            .OrderBy(o => o.Priority).ThenBy(o => o.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);
        foreach (var operation in queuedOperations)
        {
            operation.Status = SatelliteOperationStatus.Claimed;
            operation.ClaimedAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return queuedOperations.Select(CreateOperationMapper.ToResponse).ToList();
    }
}

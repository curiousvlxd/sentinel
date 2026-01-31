using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts.Simulation;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;
using Sentinel.Ground.Application.Services.Clients.OnboardAi;

namespace Sentinel.Ground.Application.Features.Operations.ExecuteOperation;

public sealed class ExecuteSatelliteOperationCommandHandler(IGroundDbContext context, IOnboardAiClient onboardAi)
    : ICommandHandler<ExecuteSatelliteOperationCommand, Result>
{
    public async Task<Result> Handle(ExecuteSatelliteOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await context.SatelliteOperations.FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);

        if (operation is null)
            return Error.NotFound("Satellite operation not found.");

        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == operation.SatelliteId, cancellationToken);

        if (satellite?.Mode is SatelliteMode.Autonomous)
            return Error.BadRequest("Operations are not allowed for satellites in Autonomous mode.");

        if (operation.Status is not SatelliteOperationStatus.Queued && operation.Status is not SatelliteOperationStatus.Claimed)
            return Error.BadRequest("Satellite operation already executed or expired");

        operation.Status = SatelliteOperationStatus.Executed;
        operation.ExecutedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            await onboardAi.ExecuteCommandAsync(new SimulationCommandRequest { CommandType = operation.Type }, cancellationToken);
        }
        catch
        {
        }

        return Result.Ok();
    }
}

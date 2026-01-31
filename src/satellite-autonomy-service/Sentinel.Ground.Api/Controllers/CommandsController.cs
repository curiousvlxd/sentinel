using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Features.Operations.CreateOperation;
using Sentinel.Ground.Application.Features.Operations.ExecuteOperation;
using Sentinel.Ground.Application.Features.Operations.GetOperationById;
using Sentinel.Ground.Application.Features.Operations.GetOperationsByMission;
using Sentinel.Ground.Application.Features.Operations.GetOperationsBySatellite;
using Sentinel.Ground.Application.Features.Operations.PullOperations;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CommandsController(IMediator mediator) : ControllerBase
{
    [HttpPost("commands/{commandId:guid}/execute")]
    public async Task<IActionResult> Execute(Guid commandId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExecuteSatelliteOperationCommand(commandId), cancellationToken);
        return result.Match();
    }

    [HttpPost("satellites/{satelliteId:guid}/commands")]
    public async Task<ActionResult<SatelliteOperationResponse>> Create(
        Guid satelliteId,
        [FromBody] CreateSatelliteOperationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSatelliteOperationCommand(satelliteId, request), cancellationToken);
        return result.Match(response => CreatedAtAction(nameof(GetCommand), new { commandId = response.Id }, response));
    }

    [HttpGet("missions/{missionId:guid}/commands")]
    public async Task<ActionResult<IReadOnlyList<SatelliteOperationResponse>>> ListByMission(
        Guid missionId,
        [FromQuery] SatelliteOperationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatelliteOperationsByMissionQuery(missionId, status), cancellationToken);
        return result.Match();
    }

    [HttpGet("satellites/{satelliteId:guid}/commands")]
    public async Task<ActionResult<IReadOnlyList<SatelliteOperationResponse>>> ListBySatellite(
        Guid satelliteId,
        [FromQuery] SatelliteOperationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatelliteOperationsBySatelliteQuery(satelliteId, status), cancellationToken);
        return result.Match();
    }

    [HttpGet("commands/{commandId:guid}")]
    public async Task<ActionResult<SatelliteOperationResponse>> GetCommand(Guid commandId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatelliteOperationByIdQuery(commandId), cancellationToken);
        return result.Match();
    }

    [HttpPost("satellites/{satelliteId:guid}/commands/pull")]
    public async Task<ActionResult<IReadOnlyList<SatelliteOperationResponse>>> Pull(
        Guid satelliteId,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 50)
    {
        var result = await mediator.Send(new PullSatelliteOperationsCommand(satelliteId, limit), cancellationToken);
        return result.Match();
    }
}

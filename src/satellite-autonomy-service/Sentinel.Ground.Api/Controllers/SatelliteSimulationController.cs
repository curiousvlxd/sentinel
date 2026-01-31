using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts.Simulation;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.Simulation.Start;
using Sentinel.Ground.Application.Features.Simulation.Stop;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/satellites/{satelliteId:guid}/sim")]
public sealed class SatelliteSimulationController(IMediator mediator) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> SimStart(
        Guid satelliteId,
        [FromBody] SimulationStartRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StartSatelliteSimulationCommand(satelliteId, request), cancellationToken);
        return result.Match(Accepted);
    }

    [HttpPost("stop")]
    public async Task<IActionResult> SimStop(Guid satelliteId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StopSatelliteSimulationCommand(satelliteId), cancellationToken);
        return result.Match(Accepted);
    }
}

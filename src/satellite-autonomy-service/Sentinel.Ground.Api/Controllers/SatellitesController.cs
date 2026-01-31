using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Features.Satellites.CreateSatellite;
using Sentinel.Ground.Application.Features.Satellites.DeleteSatellite;
using Sentinel.Ground.Application.Features.Satellites.GetSatelliteById;
using Sentinel.Ground.Application.Features.Satellites.GetSatelliteDecisions;
using Sentinel.Ground.Application.Features.Satellites.GetSatelliteMlResults;
using Sentinel.Ground.Application.Features.Satellites.GetSatellites;
using Sentinel.Ground.Application.Features.Satellites.UpdateSatellite;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/satellites")]
public sealed class SatellitesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SatelliteResponse>>> List(
        [FromQuery] Guid? missionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatellitesQuery(missionId), cancellationToken);
        return result.Match();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SatelliteResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatelliteByIdQuery(id), cancellationToken);
        return result.Match();
    }

    [HttpPost]
    public async Task<ActionResult<SatelliteResponse>> Create(
        [FromBody] CreateSatelliteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSatelliteCommand(request), cancellationToken);
        return result.Match(satellite => CreatedAtAction(nameof(Get), new { id = satellite.Id }, satellite));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SatelliteResponse>> Update(
        Guid id,
        [FromBody] UpdateSatelliteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSatelliteCommand(id, request), cancellationToken);
        return result.Match();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSatelliteCommand(id), cancellationToken);
        return result.Match();
    }

    [HttpGet("{id:guid}/decisions")]
    public async Task<ActionResult<IReadOnlyList<DecisionResponse>>> GetDecisions(
        Guid id,
        CancellationToken cancellationToken,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int limit = 100)
    {
        var result = await mediator.Send(new GetSatelliteDecisionsQuery(id, from, to, limit), cancellationToken);
        return result.Match();
    }

    [HttpGet("{id:guid}/ml-results")]
    public async Task<ActionResult<SatelliteMlResultsResponse>> GetMlResults(
        Guid id,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSatelliteMlResultsQuery(id, from, to), cancellationToken);
        return result.Match();
    }
}

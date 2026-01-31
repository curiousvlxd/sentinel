using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.Missions.AttachSatellite;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Features.Missions.CreateMission;
using Sentinel.Ground.Application.Features.Missions.DeleteMission;
using Sentinel.Ground.Application.Features.Missions.DetachSatellite;
using Sentinel.Ground.Application.Features.Missions.GetMissionById;
using Sentinel.Ground.Application.Features.Missions.GetMissions;
using Sentinel.Ground.Application.Features.Missions.UpdateMission;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/missions")]
public sealed class MissionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MissionResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMissionsQuery(), cancellationToken);
        return result.Match();
    }

    [HttpGet("{missionId:guid}")]
    public async Task<ActionResult<MissionResponse>> Get(Guid missionId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMissionByIdQuery(missionId), cancellationToken);
        return result.Match();
    }

    [HttpPost]
    public async Task<ActionResult<MissionResponse>> Create(
        [FromBody] CreateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateMissionCommand(request), cancellationToken);
        return result.Match(mission => CreatedAtAction(nameof(Get), new { missionId = mission.Id }, mission));
    }

    [HttpPut("{missionId:guid}")]
    public async Task<ActionResult<MissionResponse>> Update(
        Guid missionId,
        [FromBody] UpdateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateMissionCommand(missionId, request), cancellationToken);
        return result.Match();
    }

    [HttpDelete("{missionId:guid}")]
    public async Task<IActionResult> Delete(Guid missionId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteMissionCommand(missionId), cancellationToken);
        return result.Match();
    }

    [HttpPost("{missionId:guid}/satellites/{satelliteId:guid}")]
    public async Task<IActionResult> AttachSatellite(
        Guid missionId,
        Guid satelliteId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AttachSatelliteCommand(missionId, satelliteId), cancellationToken);
        return result.Match();
    }

    [HttpDelete("{missionId:guid}/satellites/{satelliteId:guid}")]
    public async Task<IActionResult> DetachSatellite(
        Guid missionId,
        Guid satelliteId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DetachSatelliteCommand(missionId, satelliteId), cancellationToken);
        return result.Match();
    }
}

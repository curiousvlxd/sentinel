using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/missions")]
public sealed class MissionsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MissionDto>>> List(
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var list = await context.Missions
            .OrderBy(m => m.Name)
            .Select(m => new MissionDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt.ToString("O")
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{missionId:guid}")]
    public async Task<ActionResult<MissionDto>> Get(
        Guid missionId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var m = await context.Missions
            .Where(x => x.Id == missionId)
            .Select(x => new MissionDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt.ToString("O")
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (m == null)
            return NotFound();
        return Ok(m);
    }

    [HttpPost]
    public async Task<ActionResult<MissionDto>> Create(
        [FromBody] MissionCreateRequest request,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Add(mission);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { missionId = mission.Id }, new MissionDto
        {
            Id = mission.Id,
            Name = mission.Name,
            Description = mission.Description,
            IsActive = mission.IsActive,
            CreatedAt = mission.CreatedAt.ToString("O")
        });
    }

    [HttpPut("{missionId:guid}")]
    public async Task<ActionResult<MissionDto>> Update(
        Guid missionId,
        [FromBody] MissionUpdateRequest request,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == missionId, cancellationToken);
        if (mission == null)
            return NotFound();
        mission.Name = request.Name.Trim();
        mission.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        mission.IsActive = request.IsActive;
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new MissionDto
        {
            Id = mission.Id,
            Name = mission.Name,
            Description = mission.Description,
            IsActive = mission.IsActive,
            CreatedAt = mission.CreatedAt.ToString("O")
        });
    }

    [HttpDelete("{missionId:guid}")]
    public async Task<IActionResult> Delete(
        Guid missionId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions.FirstOrDefaultAsync(m => m.Id == missionId, cancellationToken);
        if (mission == null)
            return NotFound();
        var satellites = await context.Satellites.Where(s => s.MissionId == missionId).ToListAsync(cancellationToken);
        foreach (var s in satellites)
            s.MissionId = null;
        context.RemoveMission(mission);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{missionId:guid}/satellites/{satelliteId:guid}")]
    public async Task<IActionResult> AttachSatellite(
        Guid missionId,
        Guid satelliteId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions.AnyAsync(m => m.Id == missionId, cancellationToken);
        if (!mission)
            return NotFound("Mission not found");
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == satelliteId, cancellationToken);
        if (satellite == null)
            return NotFound("Satellite not found");
        satellite.MissionId = missionId;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{missionId:guid}/satellites/{satelliteId:guid}")]
    public async Task<IActionResult> DetachSatellite(
        Guid missionId,
        Guid satelliteId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == satelliteId && s.MissionId == missionId, cancellationToken);
        if (satellite == null)
            return NotFound();
        satellite.MissionId = null;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

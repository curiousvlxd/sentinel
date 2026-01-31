using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CommandsController : ControllerBase
{
    [HttpPost("satellites/{satelliteId:guid}/commands")]
    public async Task<ActionResult<CommandDto>> Create(
        Guid satelliteId,
        [FromBody] CommandCreateRequest request,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == satelliteId, cancellationToken);
        if (satellite == null)
            return NotFound("Satellite not found");
        var cmd = new Command
        {
            Id = Guid.NewGuid(),
            SatelliteId = satelliteId,
            MissionId = satellite.MissionId,
            Type = request.Type.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(request.PayloadJson) ? null : request.PayloadJson.Trim(),
            Priority = request.Priority,
            TtlSec = request.TtlSec,
            Status = CommandStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };
        context.Add(cmd);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetCommand), new { commandId = cmd.Id }, ToDto(cmd));
    }

    [HttpGet("missions/{missionId:guid}/commands")]
    public async Task<ActionResult<IReadOnlyList<CommandDto>>> ListByMission(
        Guid missionId,
        [FromQuery] string? status,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Commands.Where(c => c.MissionId == missionId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CommandStatus>(status, true, out var st))
            query = query.Where(c => c.Status == st);
        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommandDto
            {
                Id = c.Id,
                SatelliteId = c.SatelliteId,
                MissionId = c.MissionId,
                Type = c.Type,
                PayloadJson = c.PayloadJson,
                Priority = c.Priority,
                TtlSec = c.TtlSec,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt.ToString("O"),
                ClaimedAt = c.ClaimedAt != null ? c.ClaimedAt.Value.ToString("O") : null,
                ExecutedAt = c.ExecutedAt != null ? c.ExecutedAt.Value.ToString("O") : null
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("satellites/{satelliteId:guid}/commands")]
    public async Task<ActionResult<IReadOnlyList<CommandDto>>> ListBySatellite(
        Guid satelliteId,
        [FromQuery] string? status,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Commands.Where(c => c.SatelliteId == satelliteId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CommandStatus>(status, true, out var st))
            query = query.Where(c => c.Status == st);
        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommandDto
            {
                Id = c.Id,
                SatelliteId = c.SatelliteId,
                MissionId = c.MissionId,
                Type = c.Type,
                PayloadJson = c.PayloadJson,
                Priority = c.Priority,
                TtlSec = c.TtlSec,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt.ToString("O"),
                ClaimedAt = c.ClaimedAt != null ? c.ClaimedAt.Value.ToString("O") : null,
                ExecutedAt = c.ExecutedAt != null ? c.ExecutedAt.Value.ToString("O") : null
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("commands/{commandId:guid}")]
    public async Task<ActionResult<CommandDto>> GetCommand(
        Guid commandId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var cmd = await context.Commands.FirstOrDefaultAsync(c => c.Id == commandId, cancellationToken);
        if (cmd == null)
            return NotFound();
        return Ok(ToDto(cmd));
    }

    [HttpPost("satellites/{satelliteId:guid}/commands/pull")]
    public async Task<ActionResult<IReadOnlyList<CommandDto>>> Pull(
        Guid satelliteId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 50)
    {
        var cutoff = DateTime.UtcNow;
        var queued = await context.Commands
            .Where(c => c.SatelliteId == satelliteId && c.Status == CommandStatus.Queued && c.CreatedAt.AddSeconds(c.TtlSec) >= cutoff)
            .OrderBy(c => c.Priority).ThenBy(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        foreach (var c in queued)
        {
            c.Status = CommandStatus.Claimed;
            c.ClaimedAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
        return Ok(queued.Select(ToDto).ToList());
    }

    private static CommandDto ToDto(Command c)
    {
        return new CommandDto
        {
            Id = c.Id,
            SatelliteId = c.SatelliteId,
            MissionId = c.MissionId,
            Type = c.Type,
            PayloadJson = c.PayloadJson,
            Priority = c.Priority,
            TtlSec = c.TtlSec,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt.ToString("O"),
            ClaimedAt = c.ClaimedAt?.ToString("O"),
            ExecutedAt = c.ExecutedAt?.ToString("O")
        };
    }
}

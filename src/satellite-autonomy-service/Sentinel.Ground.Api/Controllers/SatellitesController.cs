using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/satellites")]
public sealed class SatellitesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SatelliteDto>>> List(
        [FromQuery] Guid? missionId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var query = context.Satellites.AsQueryable();
        if (missionId.HasValue)
            query = query.Where(s => s.MissionId == missionId.Value);
        var list = await query
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.MissionId,
                s.Name,
                s.Status,
                s.Mode,
                s.State,
                s.LinkStatus,
                s.LastBucketStart,
                s.CreatedAt
            })
            .ToListAsync(cancellationToken);
        var missionIds = list.Where(x => x.MissionId.HasValue).Select(x => x.MissionId!.Value).Distinct().ToList();
        var missionNames = missionIds.Count > 0
            ? await context.Missions.Where(m => missionIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken)
            : new Dictionary<Guid, string>();
        var dtos = list.Select(s => new SatelliteDto
        {
            Id = s.Id,
            MissionId = s.MissionId,
            MissionName = s.MissionId != null && missionNames.TryGetValue(s.MissionId.Value, out var n) ? n : null,
            Name = s.Name,
            Status = s.Status.ToString(),
            Mode = s.Mode.ToString(),
            State = s.State.ToString(),
            LinkStatus = s.LinkStatus.ToString(),
            LastBucketStart = s.LastBucketStart?.ToString("O"),
            CreatedAt = s.CreatedAt.ToString("O")
        }).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SatelliteDto>> Get(
        Guid id,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var s = await context.Satellites
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.MissionId, x.Name, x.Status, x.Mode, x.State, x.LinkStatus, x.LastBucketStart, x.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);
        if (s == null)
            return NotFound();
        var missionName = s.MissionId != null
            ? await context.Missions.Where(m => m.Id == s.MissionId).Select(m => m.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        return Ok(new SatelliteDto
        {
            Id = s.Id,
            MissionId = s.MissionId,
            MissionName = missionName,
            Name = s.Name,
            Status = s.Status.ToString(),
            Mode = s.Mode.ToString(),
            State = s.State.ToString(),
            LinkStatus = s.LinkStatus.ToString(),
            LastBucketStart = s.LastBucketStart?.ToString("O"),
            CreatedAt = s.CreatedAt.ToString("O")
        });
    }

    [HttpPost]
    public async Task<ActionResult<SatelliteDto>> Create(
        [FromBody] SatelliteCreateRequest request,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var mode = Enum.TryParse<SatelliteMode>(request.Mode, true, out var m) ? m : SatelliteMode.Assisted;
        var satellite = new Satellite
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            MissionId = null,
            Status = SatelliteStatus.Active,
            Mode = mode,
            State = SatelliteState.Ok,
            LinkStatus = LinkStatus.Offline,
            CreatedAt = DateTime.UtcNow
        };
        context.Add(satellite);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = satellite.Id }, new SatelliteDto
        {
            Id = satellite.Id,
            MissionId = null,
            MissionName = null,
            Name = satellite.Name,
            Status = satellite.Status.ToString(),
            Mode = satellite.Mode.ToString(),
            State = satellite.State.ToString(),
            LinkStatus = satellite.LinkStatus.ToString(),
            LastBucketStart = null,
            CreatedAt = satellite.CreatedAt.ToString("O")
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SatelliteDto>> Update(
        Guid id,
        [FromBody] SatelliteUpdateRequest request,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (satellite == null)
            return NotFound();
        satellite.Name = request.Name.Trim();
        if (Enum.TryParse<SatelliteStatus>(request.Status, true, out var st))
            satellite.Status = st;
        if (Enum.TryParse<SatelliteMode>(request.Mode, true, out var mo))
            satellite.Mode = mo;
        if (Enum.TryParse<SatelliteState>(request.State, true, out var sta))
            satellite.State = sta;
        if (Enum.TryParse<LinkStatus>(request.LinkStatus, true, out var link))
            satellite.LinkStatus = link;
        await context.SaveChangesAsync(cancellationToken);
        var missionName = satellite.MissionId != null
            ? await context.Missions.Where(m => m.Id == satellite.MissionId).Select(m => m.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        return Ok(new SatelliteDto
        {
            Id = satellite.Id,
            MissionId = satellite.MissionId,
            MissionName = missionName,
            Name = satellite.Name,
            Status = satellite.Status.ToString(),
            Mode = satellite.Mode.ToString(),
            State = satellite.State.ToString(),
            LinkStatus = satellite.LinkStatus.ToString(),
            LastBucketStart = satellite.LastBucketStart?.ToString("O"),
            CreatedAt = satellite.CreatedAt.ToString("O")
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (satellite == null)
            return NotFound();
        context.RemoveSatellite(satellite);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/decisions")]
    public async Task<ActionResult<IReadOnlyList<DecisionResponse>>> GetDecisions(
        Guid id,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-1);
        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var list = await context.Decisions
            .Where(d => d.SatelliteId == id && d.BucketStart >= fromUtc && d.BucketStart <= toUtc)
            .OrderByDescending(d => d.BucketStart)
            .Take(limit)
            .Select(d => new DecisionResponse
            {
                Id = d.Id,
                SatelliteId = d.SatelliteId,
                BucketStart = d.BucketStart.ToString("O"),
                Type = d.DecisionType.ToString(),
                Reason = d.Reason,
                CreatedAt = d.CreatedAt.ToString("O")
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}/ml-results")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetMlResults(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-1);
        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var list = await context.MlHealthResults
            .Where(m => m.SatelliteId == id && m.BucketStart >= fromUtc && m.BucketStart <= toUtc)
            .OrderByDescending(m => m.BucketStart)
            .Select(m => new
            {
                m.Id,
                m.SatelliteId,
                BucketStart = m.BucketStart.ToString("O"),
                m.ModelName,
                m.ModelVersion,
                m.AnomalyScore,
                m.Confidence,
                m.PerSignalScore,
                m.TopContributors,
                CreatedAt = m.CreatedAt.ToString("O")
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}

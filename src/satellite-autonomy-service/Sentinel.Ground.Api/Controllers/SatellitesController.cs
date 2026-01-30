using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts;
using Sentinel.Core.Enums;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/satellites")]
public sealed class SatellitesController : ControllerBase
{
    [HttpGet("{id:guid}/decisions")]
    public async Task<ActionResult<IReadOnlyList<DecisionResponse>>> GetDecisions(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-1);
        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var list = await context.Decisions
            .Where(d => d.SatelliteId == id && d.BucketStart >= fromUtc && d.BucketStart <= toUtc)
            .OrderByDescending(d => d.BucketStart)
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

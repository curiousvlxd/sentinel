using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Contracts;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/decisions")]
public sealed class DecisionsController : ControllerBase
{
    [HttpGet("{decisionId:guid}")]
    public async Task<ActionResult<DecisionResponse>> Get(
        Guid decisionId,
        [FromServices] IGroundDbContext context,
        CancellationToken cancellationToken)
    {
        var d = await context.Decisions
            .Where(x => x.Id == decisionId)
            .Select(x => new DecisionResponse
            {
                Id = x.Id,
                SatelliteId = x.SatelliteId,
                BucketStart = x.BucketStart.ToString("O"),
                Type = x.DecisionType.ToString(),
                Reason = x.Reason,
                CreatedAt = x.CreatedAt.ToString("O")
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (d == null)
            return NotFound();
        return Ok(d);
    }
}

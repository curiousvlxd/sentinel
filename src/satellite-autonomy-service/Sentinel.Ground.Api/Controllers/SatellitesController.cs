using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts;
using Sentinel.Ground.Api.Services;

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
        [FromServices] DecisionRepository repo,
        CancellationToken cancellationToken)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-1);
        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var list = await repo.GetBySatelliteAsync(id, fromUtc, toUtc, cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}/ml-results")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetMlResults(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] MlResultRepository repo,
        CancellationToken cancellationToken)
    {
        var fromUtc = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-1);
        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var list = await repo.GetBySatelliteAsync(id, fromUtc, toUtc, cancellationToken);
        return Ok(list);
    }
}

using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Jobs;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/ml")]
public sealed class MlController : ControllerBase
{
    [HttpPost("evaluate/latest")]
    public async Task<IActionResult> EvaluateLatest(
        [FromQuery] Guid? satelliteId,
        [FromServices] SatelliteHealthCheckJob job,
        CancellationToken cancellationToken)
    {
        await job.RunAsync(satelliteId, cancellationToken);
        return Accepted();
    }
}

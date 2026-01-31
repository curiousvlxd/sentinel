using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts.Simulation;
using Sentinel.Core.Contracts.Telemetry;
using Sentinel.Core.Enums;
using Sentinel.Satellite.Service.Services;
using Sentinel.Satellite.Service.Services.TelemetrySimulator;

namespace Sentinel.Satellite.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TelemetryController : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<ActionResult<TelemetryIngestResponse>> Ingest(
        [FromBody] TelemetryIngestRequest request,
        [FromServices] TelemetryIngestService service,
        CancellationToken cancellationToken)
    {
        var response = await service.IngestAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("sim/start")]
    public IActionResult SimStart(
        [FromBody] SimulationStartRequest? request,
        [FromServices] TelemetrySimulatorService simulator)
    {
        simulator.Start(request?.Scenario ?? SimScenario.Normal);
        return Accepted();
    }

    [HttpPost("sim/stop")]
    public IActionResult SimStop([FromServices] TelemetrySimulatorService simulator)
    {
        simulator.Stop();
        return Accepted();
    }
}

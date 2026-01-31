using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/satellites/{satelliteId:guid}/sim")]
public sealed class SatelliteSimProxyController : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> SimStart(
        Guid satelliteId,
        [FromBody] SimStartRequest? request,
        [FromServices] IConfiguration config,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var baseUrl = config["Ground:SatelliteServiceBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return StatusCode(503, "Satellite service URL not configured");

        var client = httpClientFactory.CreateClient();
        var url = $"{baseUrl.TrimEnd('/')}/api/telemetry/sim/start";
        HttpResponseMessage response;
        if (request?.Scenario != null)
        {
            response = await client.PostAsJsonAsync(url, new { scenario = request.Scenario }, cancellationToken);
        }
        else
        {
            response = await client.PostAsync(url, null, cancellationToken);
        }

        return response.IsSuccessStatusCode ? Accepted() : StatusCode((int)response.StatusCode);
    }

    [HttpPost("stop")]
    public async Task<IActionResult> SimStop(
        Guid satelliteId,
        [FromServices] IConfiguration config,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        var baseUrl = config["Ground:SatelliteServiceBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return StatusCode(503, "Satellite service URL not configured");

        var client = httpClientFactory.CreateClient();
        var url = $"{baseUrl.TrimEnd('/')}/api/telemetry/sim/stop";
        var response = await client.PostAsync(url, null, cancellationToken);
        return response.IsSuccessStatusCode ? Accepted() : StatusCode((int)response.StatusCode);
    }
}

public sealed class SimStartRequest
{
    public string? Scenario { get; set; }
}

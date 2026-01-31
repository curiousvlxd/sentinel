using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts.Events;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Publish([FromBody] GroundEventContract evt, [FromServices] SseEventBus eventBus, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(evt, cancellationToken);
        return Accepted();
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("live")]
    public async Task GetLiveSse([FromServices] SseEventBus eventBus, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.StartAsync(cancellationToken);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await foreach (var evt in eventBus.SubscribeAsync(cancellationToken: cancellationToken))
        {
            var json = JsonSerializer.Serialize(evt, options);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}

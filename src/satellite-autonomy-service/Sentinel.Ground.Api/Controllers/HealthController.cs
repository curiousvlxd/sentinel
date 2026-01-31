using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IOptions<JsonOptions> jsonOptions) : ControllerBase
{
    [HttpGet("live")]
    public async Task GetLiveSse([FromServices] SseEventBus eventBus, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.StartAsync(cancellationToken);

        var options = jsonOptions.Value.JsonSerializerOptions;
        await foreach (var evt in eventBus.SubscribeAsync(cancellationToken: cancellationToken))
        {
            var json = JsonSerializer.Serialize(evt, options);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}

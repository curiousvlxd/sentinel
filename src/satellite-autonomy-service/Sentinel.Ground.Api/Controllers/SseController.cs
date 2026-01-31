using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sentinel.Core.Contracts.Events;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/sse")]
public sealed class SseController(IOptions<JsonOptions> jsonOptions) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

    [HttpGet("missions/{missionId:guid}")]
    public Task GetMissionStream(
        Guid missionId,
        [FromServices] SseEventBus eventBus,
        CancellationToken cancellationToken) =>
        StreamEventsAsync(eventBus.SubscribeAsync(missionId: missionId, cancellationToken: cancellationToken), cancellationToken);

    [HttpGet("satellites/{satelliteId:guid}")]
    public Task GetSatelliteStream(
        Guid satelliteId,
        [FromServices] SseEventBus eventBus,
        CancellationToken cancellationToken) =>
        StreamEventsAsync(eventBus.SubscribeAsync(satelliteId: satelliteId, cancellationToken: cancellationToken), cancellationToken);

    private async Task StreamEventsAsync(
        IAsyncEnumerable<GroundEventContract> events,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.StartAsync(cancellationToken);

        await foreach (var evt in events.WithCancellation(cancellationToken))
        {
            var data = JsonSerializer.Serialize(evt, _jsonOptions);
            await Response.WriteAsync($"data: {data}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Core.Contracts;
using Sentinel.Ground.Api.Services;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/sse")]
public sealed class SseController : ControllerBase
{
    [HttpGet("missions/{missionId:guid}")]
    public async Task GetMissionStream(
        Guid missionId,
        [FromServices] SseEventBus eventBus,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.StartAsync(cancellationToken);
        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await foreach (var evt in eventBus.SubscribeAsync(missionId: missionId, cancellationToken: cancellationToken))
        {
            var data = JsonSerializer.Serialize(new
            {
                evt.EventId,
                evt.MissionId,
                evt.SatelliteId,
                evt.Type,
                evt.Ts,
                evt.BucketStart,
                evt.Payload
            }, jsonOpts);
            await Response.WriteAsync($"data: {data}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpGet("satellites/{satelliteId:guid}")]
    public async Task GetSatelliteStream(
        Guid satelliteId,
        [FromServices] SseEventBus eventBus,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        await Response.StartAsync(cancellationToken);
        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await foreach (var evt in eventBus.SubscribeAsync(satelliteId: satelliteId, cancellationToken: cancellationToken))
        {
            var data = JsonSerializer.Serialize(new
            {
                evt.EventId,
                evt.MissionId,
                evt.SatelliteId,
                evt.Type,
                evt.Ts,
                evt.BucketStart,
                evt.Payload
            }, jsonOpts);
            await Response.WriteAsync($"data: {data}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}

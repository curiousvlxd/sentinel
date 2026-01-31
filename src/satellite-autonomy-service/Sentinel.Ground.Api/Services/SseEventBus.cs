using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Sentinel.Core.Contracts.Events;

namespace Sentinel.Ground.Api.Services;

public sealed class SseEventBus
{
    private readonly ConcurrentDictionary<string, Channel<GroundEventContract>> subscribers = new();
    private readonly ConcurrentDictionary<string, (Guid? MissionId, Guid? SatelliteId)> subscriberFilters = new();

    public async Task PublishAsync(GroundEventContract evt, CancellationToken cancellationToken = default)
    {
        if (evt.EventId == Guid.Empty) evt.EventId = Guid.NewGuid();

        if (evt.Ts == default) evt.Ts = DateTimeOffset.UtcNow;

        foreach (var (subId, ch) in subscribers)
        {
            if (!subscriberFilters.TryGetValue(subId, out var filter))
            {
                await ch.Writer.WriteAsync(evt, cancellationToken);
                continue;
            }

            if (filter.MissionId.HasValue && evt.MissionId != filter.MissionId) continue;

            if (filter.SatelliteId.HasValue && evt.SatelliteId != filter.SatelliteId) continue;

            await ch.Writer.WriteAsync(evt, cancellationToken);
        }
    }

    public async IAsyncEnumerable<GroundEventContract> SubscribeAsync(
        Guid? missionId = null,
        Guid? satelliteId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var ch = Channel.CreateUnbounded<GroundEventContract>();
        subscribers[id] = ch;
        if (missionId.HasValue || satelliteId.HasValue) subscriberFilters[id] = (missionId, satelliteId);

        try
        {
            await foreach (var evt in ch.Reader.ReadAllAsync(cancellationToken)) yield return evt;
        }
        finally
        {
            subscribers.TryRemove(id, out _);
            subscriberFilters.TryRemove(id, out _);
        }
    }
}

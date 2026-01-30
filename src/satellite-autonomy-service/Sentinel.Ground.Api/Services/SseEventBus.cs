using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Sentinel.Core.Contracts;

namespace Sentinel.Ground.Api.Services;

public sealed class SseEventBus
{
    private readonly ConcurrentDictionary<string, Channel<GroundEventContract>> _subscribers = new();

    public async Task PublishAsync(GroundEventContract evt, CancellationToken cancellationToken = default)
    {
        foreach (var ch in _subscribers.Values)
            await ch.Writer.WriteAsync(evt, cancellationToken);
    }

    public async IAsyncEnumerable<GroundEventContract> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var ch = Channel.CreateUnbounded<GroundEventContract>();
        _subscribers[id] = ch;
        try
        {
            await foreach (var evt in ch.Reader.ReadAllAsync(cancellationToken))
                yield return evt;
        }
        finally
        {
            _subscribers.TryRemove(id, out _);
        }
    }
}

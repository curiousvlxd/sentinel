using System.Net.Http.Json;
using System.Text.Json;
using Sentinel.Core.Contracts;

namespace Sentinel.Satellite.Service.Services;

public sealed class GroundEventsClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GroundEventsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task PublishAsync(GroundEventContract evt, CancellationToken cancellationToken = default)
    {
        await _http.PostAsJsonAsync("/api/events", evt, JsonOptions, cancellationToken);
    }
}

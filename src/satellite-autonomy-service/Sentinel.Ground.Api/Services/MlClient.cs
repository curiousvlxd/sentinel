using System.Net.Http.Json;
using System.Text.Json;
using Sentinel.Core.Contracts;

namespace Sentinel.Ground.Api.Services;

public sealed class MlClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public MlClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TelemetryHealthResponse?> EvaluateAsync(TelemetryBucketRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            schema_version = request.SchemaVersion,
            satellite_id = request.SatelliteId,
            bucket_start = request.BucketStart,
            bucket_sec = request.BucketSec,
            signals = request.Signals.ToDictionary(
                kv => kv.Key,
                kv => new
                {
                    mean = kv.Value.Mean,
                    min = kv.Value.Min,
                    max = kv.Value.Max,
                    std = kv.Value.Std,
                    slope = kv.Value.Slope,
                    p95 = kv.Value.P95,
                    missing_rate = kv.Value.MissingRate
                })
        };
        var response = await _http.PostAsJsonAsync("/score", payload, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TelemetryHealthResponse>(JsonOptions, cancellationToken);
        return result;
    }
}

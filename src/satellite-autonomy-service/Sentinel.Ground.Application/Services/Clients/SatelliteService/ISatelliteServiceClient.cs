using Refit;
using Sentinel.Core.Contracts.Simulation;

namespace Sentinel.Ground.Application.Services.Clients.SatelliteService;

public interface ISatelliteServiceClient
{
    [Post("/api/telemetry/sim/start")]
    Task<HttpResponseMessage> SimStartAsync([Body] SimulationStartRequest? request, CancellationToken cancellationToken = default);

    [Post("/api/telemetry/sim/stop")]
    Task<HttpResponseMessage> SimStopAsync(CancellationToken cancellationToken = default);
}

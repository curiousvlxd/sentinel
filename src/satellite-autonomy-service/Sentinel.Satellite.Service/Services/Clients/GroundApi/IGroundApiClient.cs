using Refit;
using Sentinel.Core.Contracts.Events;

namespace Sentinel.Satellite.Service.Services.Clients.GroundApi;

public interface IGroundApiClient
{
    [Post("/api/events")]
    Task PublishAsync([Body] GroundEventContract evt, CancellationToken cancellationToken = default);
}

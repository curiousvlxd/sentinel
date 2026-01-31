using Refit;
using Sentinel.Core.Contracts.Simulation;

namespace Sentinel.Ground.Application.Services.Clients.OnboardAi;

public interface IOnboardAiClient
{
    [Post("/simulation/command")]
    Task ExecuteCommandAsync([Body] SimulationCommandRequest request, CancellationToken cancellationToken = default);
}

using Refit;
using Sentinel.Core.Contracts.Telemetry;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public interface IOnboardAiClient
{
    [Post("/score")]
    Task<TelemetryHealthResponse?> ScoreAsync(OnboardAiScoreRequest request, CancellationToken cancellationToken = default);

    [Post("/score/simulate")]
    Task<TelemetryHealthResponse?> SimulateScoreAsync([Body] OnboardAiSimulateRequest request, CancellationToken cancellationToken = default);
}

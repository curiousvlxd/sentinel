using Refit;
using Sentinel.Core.Contracts;

namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public interface IOnboardAiClient
{
    [Post("/score")]
    Task<TelemetryHealthResponse?> ScoreAsync(OnboardAiScoreRequest request, CancellationToken cancellationToken = default);
}

using Microsoft.Extensions.Options;
using Sentinel.Core.Contracts.Telemetry;
using Sentinel.Core.Enums;
using Sentinel.Satellite.Service.Services.DecisionEngine.Options;

namespace Sentinel.Satellite.Service.Services.DecisionEngine;

public sealed class DecisionEngine(IOptions<DecisionEngineOptions> options)
{
    private readonly double _anomalyThreshold = options.Value.AnomalyThreshold;

    public (DecisionType Type, string Reason) Decide(TelemetryHealthResponse mlResponse)
    {
        var score = mlResponse.Ml.AnomalyScore;
        var top = mlResponse.Ml.TopContributors;

        if (score > _anomalyThreshold) return (DecisionType.RaiseAlert, $"Anomaly score {score:F2} exceeds threshold {_anomalyThreshold}");

        foreach (var c in top)
        {
            var key = c.Key.ToLowerInvariant();
            if (key.Contains("power_consumption") || key.Contains("cpu_temperature")) return (DecisionType.ReducePower, $"Top contributor: {c.Key}");

            if (key.Contains("signal_strength")) return (DecisionType.SwitchMode, "Signal strength contributor");
        }

        return (DecisionType.None, "No action");
    }
}

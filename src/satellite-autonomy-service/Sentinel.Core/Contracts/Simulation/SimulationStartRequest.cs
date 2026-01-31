using Sentinel.Core.Enums;

namespace Sentinel.Core.Contracts.Simulation;

public sealed record SimulationStartRequest
{
    public SimScenario Scenario { get; set; } = SimScenario.Normal;
}

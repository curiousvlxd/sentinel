namespace Sentinel.Core.Contracts.Simulation;

public sealed record SimulationCommandRequest
{
    public string CommandType { get; set; } = string.Empty;
}

using Sentinel.Core.Contracts.Simulation;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Simulation.Start;

public sealed record StartSatelliteSimulationCommand(Guid SatelliteId, SimulationStartRequest? Request) : ICommand<Result>;

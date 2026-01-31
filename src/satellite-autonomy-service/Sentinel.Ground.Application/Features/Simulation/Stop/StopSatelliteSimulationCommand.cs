using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Simulation.Stop;

public sealed record StopSatelliteSimulationCommand(Guid SatelliteId) : ICommand<Result>;

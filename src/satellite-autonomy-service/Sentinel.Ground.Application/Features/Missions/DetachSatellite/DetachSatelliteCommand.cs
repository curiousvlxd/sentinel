using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.DetachSatellite;

public sealed record DetachSatelliteCommand(Guid MissionId, Guid SatelliteId) : ICommand<Result>;

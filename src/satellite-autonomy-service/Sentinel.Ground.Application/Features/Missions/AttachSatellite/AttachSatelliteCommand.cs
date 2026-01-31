using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.AttachSatellite;

public sealed record AttachSatelliteCommand(Guid MissionId, Guid SatelliteId) : ICommand<Result>;

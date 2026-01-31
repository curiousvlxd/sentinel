using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.DeleteMission;

public sealed record DeleteMissionCommand(Guid MissionId) : ICommand<Result>;

using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.UpdateMission;

public sealed record UpdateMissionCommand(Guid MissionId, UpdateMissionRequest Request) : ICommand<Result<MissionResponse>>;

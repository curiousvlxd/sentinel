using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.CreateMission;

public sealed record CreateMissionCommand(CreateMissionRequest Request) : ICommand<Result<MissionResponse>>;

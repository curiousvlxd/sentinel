using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.GetMissionById;

public sealed record GetMissionByIdQuery(Guid MissionId) : IQuery<Result<MissionResponse>>;

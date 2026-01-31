using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Missions.GetMissions;

public sealed record GetMissionsQuery : IQuery<Result<IReadOnlyList<MissionResponse>>>;

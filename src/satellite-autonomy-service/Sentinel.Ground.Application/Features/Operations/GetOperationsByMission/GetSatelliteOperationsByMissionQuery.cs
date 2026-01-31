using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.GetOperationsByMission;

public sealed record GetSatelliteOperationsByMissionQuery(Guid MissionId, SatelliteOperationStatus? Status) : IQuery<Result<IReadOnlyList<SatelliteOperationResponse>>>;

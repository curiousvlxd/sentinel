using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.GetOperationsBySatellite;

public sealed record GetSatelliteOperationsBySatelliteQuery(Guid SatelliteId, SatelliteOperationStatus? Status) : IQuery<Result<IReadOnlyList<SatelliteOperationResponse>>>;

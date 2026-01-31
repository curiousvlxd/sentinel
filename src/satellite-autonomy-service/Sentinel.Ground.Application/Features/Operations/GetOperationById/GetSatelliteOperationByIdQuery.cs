using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.GetOperationById;

public sealed record GetSatelliteOperationByIdQuery(Guid OperationId) : IQuery<Result<SatelliteOperationResponse>>;

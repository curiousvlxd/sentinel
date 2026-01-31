using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.PullOperations;

public sealed record PullSatelliteOperationsCommand(Guid SatelliteId, int Limit = 50) : ICommand<Result<IReadOnlyList<SatelliteOperationResponse>>>;

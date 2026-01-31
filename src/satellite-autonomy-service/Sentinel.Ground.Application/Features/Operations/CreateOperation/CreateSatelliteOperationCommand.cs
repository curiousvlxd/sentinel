using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.CreateOperation;

public sealed record CreateSatelliteOperationCommand(Guid SatelliteId, CreateSatelliteOperationRequest Request) : ICommand<Result<SatelliteOperationResponse>>;

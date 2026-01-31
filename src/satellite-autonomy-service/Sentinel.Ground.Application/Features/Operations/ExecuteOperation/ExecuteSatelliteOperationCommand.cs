using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.ExecuteOperation;

public sealed record ExecuteSatelliteOperationCommand(Guid OperationId) : ICommand<Result>;

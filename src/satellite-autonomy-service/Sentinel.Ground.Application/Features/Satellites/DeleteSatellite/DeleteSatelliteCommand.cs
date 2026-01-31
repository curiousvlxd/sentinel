using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.DeleteSatellite;

public sealed record DeleteSatelliteCommand(Guid Id) : ICommand<Result>;

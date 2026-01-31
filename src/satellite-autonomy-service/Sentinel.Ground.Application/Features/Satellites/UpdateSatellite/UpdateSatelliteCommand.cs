using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.UpdateSatellite;

public sealed record UpdateSatelliteCommand(Guid Id, UpdateSatelliteRequest Request) : ICommand<Result<SatelliteResponse>>;

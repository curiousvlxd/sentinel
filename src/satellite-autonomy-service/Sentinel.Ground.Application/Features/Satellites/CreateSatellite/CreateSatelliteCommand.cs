using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.CreateSatellite;

public sealed record CreateSatelliteCommand(CreateSatelliteRequest Request) : ICommand<Result<SatelliteResponse>>;

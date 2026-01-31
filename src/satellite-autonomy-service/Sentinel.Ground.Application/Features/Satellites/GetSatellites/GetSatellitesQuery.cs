using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatellites;

public sealed record GetSatellitesQuery(Guid? MissionId) : IQuery<Result<IReadOnlyList<SatelliteResponse>>>;

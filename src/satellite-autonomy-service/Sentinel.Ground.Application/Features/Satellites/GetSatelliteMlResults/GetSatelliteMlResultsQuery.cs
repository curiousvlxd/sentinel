using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteMlResults;

public sealed record GetSatelliteMlResultsQuery(
    Guid SatelliteId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IQuery<Result<SatelliteMlResultsResponse>>;

using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Satellites.GetSatelliteDecisions;

public sealed record GetSatelliteDecisionsQuery(
    Guid SatelliteId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Limit = 100) : IQuery<Result<IReadOnlyList<DecisionResponse>>>;

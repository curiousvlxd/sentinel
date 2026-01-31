using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Decisions.GetDecisionById;

public sealed record GetDecisionByIdQuery(Guid DecisionId) : IQuery<Result<DecisionResponse>>;

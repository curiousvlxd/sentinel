using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Decisions.GetDecisionById;

public sealed class GetDecisionByIdQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetDecisionByIdQuery, Result<DecisionResponse>>
{
    public async Task<Result<DecisionResponse>> Handle(GetDecisionByIdQuery request, CancellationToken cancellationToken)
    {
        var decision = await context.Decisions
            .Where(x => x.Id == request.DecisionId)
            .ToResponse()
            .FirstOrDefaultAsync(cancellationToken);
        return decision is null ? Error.NotFound("Decision not found.") : decision;
    }
}

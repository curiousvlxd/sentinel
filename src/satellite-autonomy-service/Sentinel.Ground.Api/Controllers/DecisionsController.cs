using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;
using Sentinel.Ground.Application.Features.Decisions.GetDecisionById;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/decisions")]
public sealed class DecisionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{decisionId:guid}")]
    public async Task<ActionResult<DecisionResponse>> Get(Guid decisionId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDecisionByIdQuery(decisionId), cancellationToken);
        return result.Match();
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Api.Extensions;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Features.OperationTemplates.CreateOperationTemplate;
using Sentinel.Ground.Application.Features.OperationTemplates.DeleteOperationTemplate;
using Sentinel.Ground.Application.Features.OperationTemplates.GetOperationTemplateById;
using Sentinel.Ground.Application.Features.OperationTemplates.ListOperationTemplates;
using Sentinel.Ground.Application.Features.OperationTemplates.UpdateOperationTemplate;

namespace Sentinel.Ground.Api.Controllers;

[ApiController]
[Route("api/commands/templates")]
public sealed class CommandTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CommandTemplateResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListOperationTemplatesQuery(), cancellationToken);
        return result.Match();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommandTemplateResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOperationTemplateByIdQuery(id), cancellationToken);
        return result.Match();
    }

    [HttpPost]
    public async Task<ActionResult<CommandTemplateResponse>> Create(
        [FromBody] CommandTemplateCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateOperationTemplateCommand(request), cancellationToken);
        return result.Match(response => CreatedAtAction(nameof(Get), new { id = response.Id }, response));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommandTemplateResponse>> Update(
        Guid id,
        [FromBody] CommandTemplateUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateOperationTemplateCommand(id, request), cancellationToken);
        return result.Match();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteOperationTemplateCommand(id), cancellationToken);
        return result.Match();
    }
}

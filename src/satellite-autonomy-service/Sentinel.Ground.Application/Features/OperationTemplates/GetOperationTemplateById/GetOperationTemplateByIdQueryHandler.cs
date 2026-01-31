using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.GetOperationTemplateById;

public sealed class GetOperationTemplateByIdQueryHandler(IGroundDbContext context)
    : IQueryHandler<GetOperationTemplateByIdQuery, Result<CommandTemplateResponse>>
{
    public async Task<Result<CommandTemplateResponse>> Handle(GetOperationTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await context.CommandTemplates.Include(x => x.Fields).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return template is null ? Error.NotFound("Command template not found.") : GetOperationTemplateByIdMapper.ToResponse(template);
    }
}

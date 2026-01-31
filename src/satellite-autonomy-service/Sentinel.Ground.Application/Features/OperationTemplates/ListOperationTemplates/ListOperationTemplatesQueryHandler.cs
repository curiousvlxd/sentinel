using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.ListOperationTemplates;

public sealed class ListOperationTemplatesQueryHandler(IGroundDbContext context)
    : IQueryHandler<ListOperationTemplatesQuery, Result<IReadOnlyList<CommandTemplateResponse>>>
{
    public async Task<Result<IReadOnlyList<CommandTemplateResponse>>> Handle(ListOperationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await context.CommandTemplates.Include(t => t.Fields).OrderBy(t => t.Type).ToListAsync(cancellationToken);
        return templates.Select(ListOperationTemplatesMapper.ToResponse).ToList();
    }
}

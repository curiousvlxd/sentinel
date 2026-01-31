using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.DeleteOperationTemplate;

public sealed class DeleteOperationTemplateCommandHandler(IGroundDbContext context)
    : ICommandHandler<DeleteOperationTemplateCommand, Result>
{
    public async Task<Result> Handle(DeleteOperationTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.CommandTemplates.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (template is null)
            return Error.NotFound("Command template not found.");

        context.RemoveCommandTemplate(template);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

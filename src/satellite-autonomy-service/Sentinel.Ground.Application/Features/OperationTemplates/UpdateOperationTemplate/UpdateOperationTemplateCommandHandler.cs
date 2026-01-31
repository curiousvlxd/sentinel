using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.UpdateOperationTemplate;

public sealed class UpdateOperationTemplateCommandHandler(IGroundDbContext context)
    : ICommandHandler<UpdateOperationTemplateCommand, Result<CommandTemplateResponse>>
{
    public async Task<Result<CommandTemplateResponse>> Handle(UpdateOperationTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.CommandTemplates.Include(t => t.Fields).FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (template is null)
            return Error.NotFound("Command template not found.");

        template.Description = request.Request.Description?.Trim() ?? string.Empty;
        template.Fields.Clear();
        foreach (var f in request.Request.PayloadSchema ?? [])
        {
            template.Fields.Add(new CommandTemplateField
            {
                Id = Guid.NewGuid(),
                CommandTemplateId = template.Id,
                Name = f.Name?.Trim() ?? string.Empty,
                FieldType = f.FieldType?.Trim() ?? "number",
                Unit = (Units)f.Unit,
                DefaultValue = string.IsNullOrWhiteSpace(f.DefaultValue) ? null : f.DefaultValue.Trim()
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return UpdateOperationTemplateMapper.ToResponse(template);
    }
}

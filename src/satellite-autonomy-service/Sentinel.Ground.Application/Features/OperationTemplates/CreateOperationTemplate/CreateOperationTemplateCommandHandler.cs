using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.CreateOperationTemplate;

public sealed class CreateOperationTemplateCommandHandler(IGroundDbContext context)
    : ICommandHandler<CreateOperationTemplateCommand, Result<CommandTemplateResponse>>
{
    public async Task<Result<CommandTemplateResponse>> Handle(CreateOperationTemplateCommand request, CancellationToken cancellationToken)
    {
        var exists = await context.CommandTemplates.AnyAsync(t => t.Type == request.Request.Type.Trim(), cancellationToken);
        if (exists)
            return Error.Conflict("Template with this Type already exists.");

        var template = new CommandTemplate
        {
            Id = Guid.NewGuid(),
            Type = request.Request.Type.Trim(),
            Description = request.Request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };
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

        context.Add(template);
        await context.SaveChangesAsync(cancellationToken);
        return CreateOperationTemplateMapper.ToResponse(template);
    }
}

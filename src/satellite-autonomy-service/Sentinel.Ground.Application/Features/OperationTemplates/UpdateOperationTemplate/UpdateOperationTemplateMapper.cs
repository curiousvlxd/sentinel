using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Mappers;

namespace Sentinel.Ground.Application.Features.OperationTemplates.UpdateOperationTemplate;

public static class UpdateOperationTemplateMapper
{
    public static CommandTemplateResponse ToResponse(CommandTemplate template) =>
        CreateOperationTemplateMapper.ToResponseWithFields(template);
}

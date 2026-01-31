using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Mappers;

namespace Sentinel.Ground.Application.Features.OperationTemplates.GetOperationTemplateById;

public static class GetOperationTemplateByIdMapper
{
    public static CommandTemplateResponse ToResponse(CommandTemplate template) =>
        CreateOperationTemplateMapper.ToResponseWithFields(template);
}

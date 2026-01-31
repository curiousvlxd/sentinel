using Riok.Mapperly.Abstractions;
using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;

namespace Sentinel.Ground.Application.Features.OperationTemplates.Common.Mappers;

[Mapper]
public static partial class CreateOperationTemplateMapper
{
    [MapProperty(nameof(CommandTemplate.CreatedAt), nameof(CommandTemplateResponse.CreatedAt), StringFormat = "O")]
    public static partial CommandTemplateResponse ToResponse(this CommandTemplate template);

    public static CommandTemplateResponse ToResponseWithFields(CommandTemplate t)
    {
        var response = ToResponse(t);
        response.PayloadSchema = t.Fields.OrderBy(f => f.Name).Select(f => new PayloadFieldContract
        {
            Id = f.Id,
            Name = f.Name,
            FieldType = f.FieldType,
            Unit = (short)f.Unit,
            DefaultValue = f.DefaultValue
        }).ToList();
        return response;
    }
}

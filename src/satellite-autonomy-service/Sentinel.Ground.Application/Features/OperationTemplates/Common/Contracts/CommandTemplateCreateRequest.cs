namespace Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;

public sealed class CommandTemplateCreateRequest
{
    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<PayloadFieldCreateContract> PayloadSchema { get; set; } = [];
}

public sealed class PayloadFieldCreateContract
{
    public string Name { get; set; } = string.Empty;

    public string FieldType { get; set; } = "number";

    public short Unit { get; set; }

    public string? DefaultValue { get; set; }
}

namespace Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;

public sealed class CommandTemplateResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;

    public IReadOnlyList<PayloadFieldContract> PayloadSchema { get; set; } = [];
}

public sealed class PayloadFieldContract
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FieldType { get; set; } = "number";

    public short Unit { get; set; }

    public string? DefaultValue { get; set; }
}

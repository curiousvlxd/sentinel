using Sentinel.Core.Enums;

namespace Sentinel.Core.Entities;

public sealed class CommandTemplateField
{
    public Guid Id { get; set; }

    public Guid CommandTemplateId { get; set; }

    public CommandTemplate? CommandTemplate { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FieldType { get; set; } = "number";

    public Units Unit { get; set; }

    public string? DefaultValue { get; set; }
}

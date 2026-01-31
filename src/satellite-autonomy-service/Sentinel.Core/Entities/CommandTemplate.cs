namespace Sentinel.Core.Entities;

public sealed class CommandTemplate
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<CommandTemplateField> Fields { get; set; } = new List<CommandTemplateField>();
}

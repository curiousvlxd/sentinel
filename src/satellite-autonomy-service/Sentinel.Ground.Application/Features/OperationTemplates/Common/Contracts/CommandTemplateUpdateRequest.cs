namespace Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;

public sealed class CommandTemplateUpdateRequest
{
    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<PayloadFieldCreateContract> PayloadSchema { get; set; } = [];
}

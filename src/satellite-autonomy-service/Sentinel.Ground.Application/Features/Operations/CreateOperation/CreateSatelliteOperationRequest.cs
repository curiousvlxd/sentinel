namespace Sentinel.Ground.Application.Features.Operations.CreateOperation;

public sealed record CreateSatelliteOperationRequest
{
    public Guid? CommandTemplateId { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Priority { get; set; }

    public int TtlSec { get; set; }

    public string? PayloadJson { get; set; }
}

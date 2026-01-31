using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;
using Sentinel.Ground.Application.Features.Operations.Common.Mappers;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.Operations.CreateOperation;

public sealed class CreateSatelliteOperationCommandHandler(IGroundDbContext context)
    : ICommandHandler<CreateSatelliteOperationCommand, Result<SatelliteOperationResponse>>
{
    public async Task<Result<SatelliteOperationResponse>> Handle(CreateSatelliteOperationCommand request, CancellationToken cancellationToken)
    {
        var satellite = await context.Satellites.FirstOrDefaultAsync(s => s.Id == request.SatelliteId, cancellationToken);
        if (satellite is null)
            return Error.NotFound("Satellite not found.");

        if (satellite.Mode is SatelliteMode.Autonomous)
            return Error.BadRequest("Operations are not allowed for satellites in Autonomous mode. Use Assisted or Manual.");

        var type = request.Request.Type?.Trim() ?? string.Empty;
        var templateId = request.Request.CommandTemplateId;
        if (templateId.HasValue)
        {
            var template = await context.CommandTemplates.FirstOrDefaultAsync(t => t.Id == templateId.Value, cancellationToken);
            if (template is not null)
                type = template.Type;
        }

        if (string.IsNullOrEmpty(type))
            return Error.BadRequest("Type or CommandTemplateId required.");

        var operation = SatelliteOperation.Create(
            request.SatelliteId,
            satellite.MissionId,
            templateId,
            type,
            request.Request.PayloadJson,
            request.Request.Priority,
            request.Request.TtlSec);

        context.Add(operation);
        await context.SaveChangesAsync(cancellationToken);
        return CreateOperationMapper.ToResponse(operation);
    }
}

using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.GetOperationTemplateById;

public sealed record GetOperationTemplateByIdQuery(Guid Id) : IQuery<Result<CommandTemplateResponse>>;

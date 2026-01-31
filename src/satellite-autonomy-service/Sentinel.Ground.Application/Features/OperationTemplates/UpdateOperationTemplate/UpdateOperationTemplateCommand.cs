using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.UpdateOperationTemplate;

public sealed record UpdateOperationTemplateCommand(Guid Id, CommandTemplateUpdateRequest Request) : ICommand<Result<CommandTemplateResponse>>;

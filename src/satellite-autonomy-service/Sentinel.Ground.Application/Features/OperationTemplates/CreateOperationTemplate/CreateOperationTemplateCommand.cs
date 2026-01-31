using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.CreateOperationTemplate;

public sealed record CreateOperationTemplateCommand(CommandTemplateCreateRequest Request) : ICommand<Result<CommandTemplateResponse>>;

using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.DeleteOperationTemplate;

public sealed record DeleteOperationTemplateCommand(Guid Id) : ICommand<Result>;

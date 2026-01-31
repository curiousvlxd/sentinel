using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Features.OperationTemplates.Common.Contracts;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Application.Features.OperationTemplates.ListOperationTemplates;

public sealed record ListOperationTemplatesQuery : IQuery<Result<IReadOnlyList<CommandTemplateResponse>>>;

using MediatR;

namespace Sentinel.Core.Messaging;

public interface ICommand : IRequest, ICommandBase
{
}

public interface ICommand<TResponse> : IRequest<TResponse>, ICommandBase
{
}

public interface ICommandBase
{
}

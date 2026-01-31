using MediatR;

namespace Sentinel.Core.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

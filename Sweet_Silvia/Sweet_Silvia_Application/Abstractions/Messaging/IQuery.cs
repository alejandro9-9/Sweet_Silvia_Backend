using MediatR;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

using MediatR;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}

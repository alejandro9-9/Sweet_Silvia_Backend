using MediatR;
using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>, IBaseCommand
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}

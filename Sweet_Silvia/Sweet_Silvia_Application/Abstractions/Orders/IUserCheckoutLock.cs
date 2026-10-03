namespace Sweet_Silvia_Application.Abstractions.Orders;

public interface IUserCheckoutLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid userId, CancellationToken cancellationToken = default);
}

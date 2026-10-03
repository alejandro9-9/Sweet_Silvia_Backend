namespace Sweet_Silvia_Application.Abstractions.Coupons;

public interface ICouponUsageLock
{
    Task<IAsyncDisposable> AcquireAsync(Guid couponId, CancellationToken cancellationToken = default);
}

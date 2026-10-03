using Sweet_Silvia_Domain.Order;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;

namespace Sweet_Silvia_Application.Coupon;

internal enum CouponUsageLimitExceeded
{
    Total,
    PerUser
}

internal static class CouponUsageLimitGuard
{
    public static CouponUsageLimitExceeded? GetExceededLimit(
        CouponEntity coupon,
        Guid userId,
        IQueryable<OrderEntity> orders,
        IQueryable<CouponUsageEntity> couponUsages,
        Guid? excludedOrderId = null)
    {
        var activeOrders = orders.Where(order => order.CouponId == coupon.Id &&
            (order.Status == OrderStatus.PendingReceipt || order.Status == OrderStatus.ReceiptInReview));
        if (excludedOrderId.HasValue)
        {
            activeOrders = activeOrders.Where(order => order.Id != excludedOrderId.Value);
        }

        var couponUses = couponUsages.Where(usage => usage.CouponId == coupon.Id);
        return GetExceededLimit(
            coupon.MaximumTotalUses,
            coupon.MaximumUsesPerUser,
            couponUses.Count(),
            activeOrders.Count(),
            couponUses.Count(usage => usage.UserId == userId),
            activeOrders.Count(order => order.UserId == userId));
    }

    public static CouponUsageLimitExceeded? GetExceededLimit(
        int? maximumTotalUses,
        int? maximumUsesPerUser,
        int completedTotalUses,
        int reservedTotalUses,
        int completedUserUses,
        int reservedUserUses)
    {
        if (maximumTotalUses.HasValue && completedTotalUses + reservedTotalUses >= maximumTotalUses.Value)
        {
            return CouponUsageLimitExceeded.Total;
        }

        if (maximumUsesPerUser.HasValue && completedUserUses + reservedUserUses >= maximumUsesPerUser.Value)
        {
            return CouponUsageLimitExceeded.PerUser;
        }

        return null;
    }
}

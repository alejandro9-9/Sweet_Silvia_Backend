using Sweet_Silvia_Application.Coupon;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;

namespace Sweet_Silvia_Tests;

public sealed class CouponUsageLimitTests
{
    [Fact]
    public void Checkout_reserves_coupon_capacity_before_payment_is_completed()
    {
        var exceeded = CouponUsageLimitGuard.GetExceededLimit(
            maximumTotalUses: 2,
            maximumUsesPerUser: 1,
            completedTotalUses: 0,
            reservedTotalUses: 2,
            completedUserUses: 0,
            reservedUserUses: 1);

        Assert.Equal(CouponUsageLimitExceeded.Total, exceeded);
    }

    [Fact]
    public void Settling_an_existing_reservation_does_not_consume_capacity_twice()
    {
        var exceeded = CouponUsageLimitGuard.GetExceededLimit(
            maximumTotalUses: 2,
            maximumUsesPerUser: 1,
            completedTotalUses: 0,
            reservedTotalUses: 1,
            completedUserUses: 0,
            reservedUserUses: 0);

        Assert.Null(exceeded);
    }

    [Fact]
    public void User_limit_counts_both_completed_uses_and_open_orders()
    {
        var exceeded = CouponUsageLimitGuard.GetExceededLimit(
            maximumTotalUses: null,
            maximumUsesPerUser: 2,
            completedTotalUses: 1,
            reservedTotalUses: 5,
            completedUserUses: 1,
            reservedUserUses: 1);

        Assert.Equal(CouponUsageLimitExceeded.PerUser, exceeded);
    }

    [Fact]
    public void Pending_order_reserves_a_real_coupon_slot_until_settlement()
    {
        var userId = Guid.NewGuid();
        var coupon = CouponEntity.Create(
            CouponCode.Create("SAVE10"),
            "Prueba",
            CouponScope.Order,
            DiscountType.Percentage,
            10,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));
        coupon.ConfigureRules(null, null, null, null, 1, 1);
        var order = OrderEntity.Create(
            userId,
            null,
            null,
            coupon.Id,
            null,
            Money.Create(100m),
            Money.Create(10m),
            Money.Zero(),
            Money.Create(90m));
        var orders = new[] { order }.AsQueryable();
        var usages = Array.Empty<CouponUsageEntity>().AsQueryable();

        var checkoutLimit = CouponUsageLimitGuard.GetExceededLimit(coupon, userId, orders, usages);
        var settlementLimit = CouponUsageLimitGuard.GetExceededLimit(coupon, userId, orders, usages, order.Id);

        Assert.Equal(CouponUsageLimitExceeded.Total, checkoutLimit);
        Assert.Null(settlementLimit);
    }
}

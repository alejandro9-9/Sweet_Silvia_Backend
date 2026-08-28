using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.CouponUsage;

public sealed class CouponUsage : Entity
{
    private CouponUsage() { }

    public CouponUsage(Guid id, Guid couponId, Guid userId, Guid orderId, CouponScope scope, Guid? orderItemId, int? appliedQuantity, Money discountAmount, DateTimeOffset usedAt)
        : base(id)
    {
        CouponId = couponId;
        UserId = userId;
        OrderId = orderId;
        Scope = scope;
        OrderItemId = orderItemId;
        AppliedQuantity = appliedQuantity;
        DiscountAmount = discountAmount;
        UsedAt = usedAt;
    }

    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public CouponScope Scope { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public int? AppliedQuantity { get; private set; }
    public Money DiscountAmount { get; private set; } = null!;
    public DateTimeOffset UsedAt { get; private set; }

    public static CouponUsage Create(Guid couponId, Guid userId, Guid orderId, CouponScope scope, Guid? orderItemId, int? appliedQuantity, Money discountAmount)
    {
        if (couponId == Guid.Empty)
        {
            throw new ArgumentException("Coupon id is required.", nameof(couponId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (orderItemId == Guid.Empty)
        {
            throw new ArgumentException("Order item id is invalid.", nameof(orderItemId));
        }

        if (appliedQuantity <= 0)
        {
            throw new ArgumentException("Applied quantity should be greater than zero.", nameof(appliedQuantity));
        }

        if (discountAmount is null)
        {
            throw new ArgumentNullException(nameof(discountAmount));
        }

        ValidateScope(scope, orderItemId, appliedQuantity);

        return new CouponUsage(Guid.NewGuid(), couponId, userId, orderId, scope, orderItemId, appliedQuantity, discountAmount, DateTimeOffset.UtcNow);
    }

    private static void ValidateScope(CouponScope scope, Guid? orderItemId, int? appliedQuantity)
    {
        if (scope == CouponScope.Item && (!orderItemId.HasValue || !appliedQuantity.HasValue))
        {
            throw new ArgumentException("Item coupons should define order item id and applied quantity.");
        }

        if (scope == CouponScope.Order && (orderItemId.HasValue || appliedQuantity.HasValue))
        {
            throw new ArgumentException("Order coupons should not define order item id or applied quantity.");
        }
    }
}

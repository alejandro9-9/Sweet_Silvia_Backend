using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.CartAppliedCoupon;

public sealed class CartAppliedCoupon : Entity
{
    private CartAppliedCoupon() { }

    public CartAppliedCoupon(Guid id, Guid cartId, Guid couponId, CouponScope scope, Guid? cartItemId, int? appliedQuantity, Money estimatedDiscountAmount, DateTimeOffset appliedAt)
        : base(id)
    {
        CartId = cartId;
        CouponId = couponId;
        Scope = scope;
        CartItemId = cartItemId;
        AppliedQuantity = appliedQuantity;
        EstimatedDiscountAmount = estimatedDiscountAmount;
        AppliedAt = appliedAt;
    }

    public Guid CartId { get; private set; }
    public Guid CouponId { get; private set; }
    public CouponScope Scope { get; private set; }
    public Guid? CartItemId { get; private set; }
    public int? AppliedQuantity { get; private set; }
    public Money EstimatedDiscountAmount { get; private set; } = null!;
    public DateTimeOffset AppliedAt { get; private set; }

    public static CartAppliedCoupon Create(Guid cartId, Guid couponId, CouponScope scope, Guid? cartItemId, int? appliedQuantity, Money estimatedDiscountAmount)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException("Cart id is required.", nameof(cartId));
        }

        if (couponId == Guid.Empty)
        {
            throw new ArgumentException("Coupon id is required.", nameof(couponId));
        }

        if (cartItemId == Guid.Empty)
        {
            throw new ArgumentException("Cart item id is invalid.", nameof(cartItemId));
        }

        if (appliedQuantity <= 0)
        {
            throw new ArgumentException("Applied quantity should be greater than zero.", nameof(appliedQuantity));
        }

        if (estimatedDiscountAmount is null)
        {
            throw new ArgumentNullException(nameof(estimatedDiscountAmount));
        }

        ValidateScope(scope, cartItemId, appliedQuantity);

        return new CartAppliedCoupon(Guid.NewGuid(), cartId, couponId, scope, cartItemId, appliedQuantity, estimatedDiscountAmount, DateTimeOffset.UtcNow);
    }

    private static void ValidateScope(CouponScope scope, Guid? cartItemId, int? appliedQuantity)
    {
        if (scope == CouponScope.Item && (!cartItemId.HasValue || !appliedQuantity.HasValue))
        {
            throw new ArgumentException("Item coupons should define cart item id and applied quantity.");
        }

        if (scope == CouponScope.Order && (cartItemId.HasValue || appliedQuantity.HasValue))
        {
            throw new ArgumentException("Order coupons should not define cart item id or applied quantity.");
        }
    }
}

using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Coupon;

namespace Sweet_Silvia_Application.CartAppliedCoupon;

public sealed record CartAppliedCouponResponse(Guid Id, Guid CartId, Guid CouponId, CouponScope Scope, Guid? CartItemId, int? AppliedQuantity, decimal EstimatedDiscountAmount, string Currency, DateTimeOffset AppliedAt);

public sealed record ApplyCouponToCartCommand(Guid CartId, Guid UserId, Guid CouponId, CouponScope Scope, Guid? CartItemId, int? AppliedQuantity) : ICommand<CartAppliedCouponResponse>;

public sealed record RemoveCouponFromCartCommand(Guid CartAppliedCouponId) : ICommand;

public sealed record GetAppliedCouponsByCartQuery(Guid CartId) : IQuery<List<CartAppliedCouponResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Coupon;

namespace Sweet_Silvia_Application.CouponUsage;

public sealed record CouponUsageResponse(Guid Id, Guid CouponId, Guid UserId, Guid OrderId, CouponScope Scope, Guid? OrderItemId, int? AppliedQuantity, decimal DiscountAmount, string Currency, DateTimeOffset UsedAt);

public sealed record RegisterCouponUsageCommand(Guid CouponId, Guid UserId, Guid OrderId, CouponScope Scope, Guid? OrderItemId, int? AppliedQuantity, decimal DiscountAmount, string Currency = "PEN") : ICommand<Guid>;

public sealed record GetCouponUsagesByCouponQuery(Guid CouponId) : IQuery<List<CouponUsageResponse>>;

public sealed record GetCouponUsagesByUserQuery(Guid UserId) : IQuery<List<CouponUsageResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Coupon;

namespace Sweet_Silvia_Application.Coupon;

public sealed record CouponResponse(Guid Id, string Code, string Name, CouponScope Scope, DiscountType DiscountType, decimal DiscountValue, decimal? MinimumPurchaseAmount, int? MinimumItemsQuantity, int? MaximumDiscountedItemsQuantity, CouponApplicationStrategy? ApplicationStrategy, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsActive, int? MaximumTotalUses, int? MaximumUsesPerUser);

public sealed record CreateCouponCommand(string Code, string Name, CouponScope Scope, DiscountType DiscountType, decimal DiscountValue, DateTimeOffset StartDate, DateTimeOffset EndDate) : ICommand<Guid>;

public sealed record ConfigureCouponRulesCommand(Guid Id, decimal? MinimumPurchaseAmount, int? MinimumItemsQuantity, int? MaximumDiscountedItemsQuantity, CouponApplicationStrategy? ApplicationStrategy, int? MaximumTotalUses, int? MaximumUsesPerUser) : ICommand;

public sealed record DeleteCouponCommand(Guid Id) : ICommand;

public sealed record GetCouponByIdQuery(Guid Id) : IQuery<CouponResponse>;

public sealed record GetCouponByCodeQuery(string Code) : IQuery<CouponResponse>;

public sealed record GetCouponsQuery(bool OnlyActive = true) : IQuery<List<CouponResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Cart;

public sealed record CartResponse(Guid Id, Guid UserId, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, List<CartItemSummaryResponse> Items, List<CartAppliedCouponSummaryResponse> AppliedCoupons);

public sealed record CartItemSummaryResponse(Guid Id, Guid ProductVariantId, int Quantity);

public sealed record CartAppliedCouponSummaryResponse(Guid Id, Guid CouponId, Guid? CartItemId, int? AppliedQuantity, decimal EstimatedDiscountAmount, string Currency);

public sealed record CreateCartCommand(Guid UserId) : ICommand<Guid>;

public sealed record AddCartItemCommand(Guid CartId, Guid ProductVariantId, int Quantity) : ICommand;

public sealed record UpdateCartItemQuantityCommand(Guid CartId, Guid ProductVariantId, int Quantity) : ICommand;

public sealed record DecreaseCartItemQuantityCommand(Guid CartId, Guid ProductVariantId, int Quantity) : ICommand;

public sealed record RemoveCartItemCommand(Guid CartId, Guid ProductVariantId) : ICommand;

public sealed record ClearCartCommand(Guid CartId) : ICommand;

public sealed record DeactivateCartCommand(Guid CartId) : ICommand;

public sealed record GetActiveCartByUserQuery(Guid UserId) : IQuery<CartResponse>;

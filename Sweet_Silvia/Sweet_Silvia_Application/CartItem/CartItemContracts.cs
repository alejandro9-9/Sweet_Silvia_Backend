using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.CartItem;

public sealed record CartItemResponse(Guid Id, Guid CartId, Guid ProductVariantId, int Quantity, DateTimeOffset AddedAt, DateTimeOffset? UpdatedAt);

public sealed record GetCartItemsByCartQuery(Guid CartId) : IQuery<List<CartItemResponse>>;

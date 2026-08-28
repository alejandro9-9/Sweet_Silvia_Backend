using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.OrderItem;

public sealed record OrderItemResponse(Guid Id, Guid OrderId, Guid ProductVariantId, string ProductName, string Sku, string Size, string Color, int Quantity, decimal UnitPrice, decimal UnitDiscount, decimal Subtotal, string Currency);

public sealed record GetOrderItemsByOrderQuery(Guid OrderId) : IQuery<List<OrderItemResponse>>;

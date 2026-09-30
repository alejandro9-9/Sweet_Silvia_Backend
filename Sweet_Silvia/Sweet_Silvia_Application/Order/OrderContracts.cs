using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Order;

namespace Sweet_Silvia_Application.Order;

public sealed record OrderResponse(Guid Id, Guid UserId, Guid? AddressId, Guid? ShippingAgencyId, Guid? CouponId, int? CouponAppliedQuantity, OrderStatus Status, decimal Subtotal, decimal DiscountTotal, decimal ShippingCost, decimal Total, string Currency, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record CreateOrderCommand(Guid UserId, Guid? AddressId, Guid? ShippingRateId, Guid? ShippingAgencyId = null, Guid? ShippingDepartmentId = null, Guid? ShippingProvinceId = null, Guid? ShippingDistrictId = null, Guid? CouponId = null, decimal? ExpectedOlvaShippingCost = null) : ICommand<Guid>;

public sealed record AddOrderItemCommand(Guid OrderId, Guid ProductVariantId, string ProductName, string Sku, string Size, string Color, int Quantity, decimal UnitPrice, decimal UnitDiscount, decimal Subtotal, string Currency = "PEN") : ICommand;

public sealed record ChangeOrderStatusCommand(Guid OrderId, OrderStatus Status, Guid? ChangedByUserId, string? Observation) : ICommand;

public sealed record GetOrderByIdQuery(Guid Id) : IQuery<OrderResponse>;

public sealed record GetOrdersQuery(OrderStatus? Status = null) : IQuery<List<OrderResponse>>;

public sealed record GetOrdersByUserQuery(Guid UserId) : IQuery<List<OrderResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Order;

namespace Sweet_Silvia_Application.OrderStatusHistory;

public sealed record OrderStatusHistoryResponse(Guid Id, Guid OrderId, OrderStatus? PreviousStatus, OrderStatus NewStatus, Guid? ChangedByUserId, DateTimeOffset ChangedAt, string? Observation);

public sealed record GetOrderStatusHistoryQuery(Guid OrderId) : IQuery<List<OrderStatusHistoryResponse>>;

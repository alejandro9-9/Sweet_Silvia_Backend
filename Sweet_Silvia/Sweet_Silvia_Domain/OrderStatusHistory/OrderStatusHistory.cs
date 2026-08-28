using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Order;

namespace Sweet_Silvia_Domain.OrderStatusHistory;

public sealed class OrderStatusHistory : Entity
{
    private OrderStatusHistory() { }

    public OrderStatusHistory(Guid id, Guid orderId, OrderStatus? previousStatus, OrderStatus newStatus, Guid? changedByUserId, DateTimeOffset changedAt, string? observation)
        : base(id)
    {
        OrderId = orderId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        ChangedAt = changedAt;
        Observation = observation;
    }

    public Guid OrderId { get; private set; }
    public OrderStatus? PreviousStatus { get; private set; }
    public OrderStatus NewStatus { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Observation { get; private set; }

    public static OrderStatusHistory Create(Guid orderId, OrderStatus? previousStatus, OrderStatus newStatus, Guid? changedByUserId, string? observation = null)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (changedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Changed by user id is invalid.", nameof(changedByUserId));
        }

        return new OrderStatusHistory(Guid.NewGuid(), orderId, previousStatus, newStatus, changedByUserId, DateTimeOffset.UtcNow, observation?.Trim());
    }
}

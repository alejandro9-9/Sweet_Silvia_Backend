using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Order;

public sealed record OrderStatusChangedEvent(Guid Id, OrderStatus PreviousStatus, OrderStatus NewStatus, Guid? ChangedByUserId, string? Observation) : IDomainEvent;

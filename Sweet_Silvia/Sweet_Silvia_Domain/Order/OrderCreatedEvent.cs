using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Order;

public sealed record OrderCreatedEvent(Guid Id) : IDomainEvent;

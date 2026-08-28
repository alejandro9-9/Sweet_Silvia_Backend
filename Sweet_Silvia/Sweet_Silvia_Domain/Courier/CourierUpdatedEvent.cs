using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Courier;

public sealed record CourierUpdatedEvent(Guid Id) : IDomainEvent;

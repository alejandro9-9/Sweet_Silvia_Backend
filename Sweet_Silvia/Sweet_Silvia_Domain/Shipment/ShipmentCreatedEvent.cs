using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Shipment;

public sealed record ShipmentCreatedEvent(Guid Id) : IDomainEvent;

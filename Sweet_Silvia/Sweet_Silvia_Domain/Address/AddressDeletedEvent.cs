using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Address;

public sealed record AddressDeletedEvent(Guid Id) : IDomainEvent;

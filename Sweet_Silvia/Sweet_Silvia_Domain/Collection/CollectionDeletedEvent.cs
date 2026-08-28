using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Collection;

public sealed record CollectionDeletedEvent(Guid Id) : IDomainEvent;

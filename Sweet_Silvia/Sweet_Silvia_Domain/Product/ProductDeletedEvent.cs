using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Product;

public sealed record ProductDeletedEvent(Guid Id) : IDomainEvent;

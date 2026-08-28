using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.ProductVariant;

public sealed record ProductVariantCreatedEvent(Guid Id) : IDomainEvent;

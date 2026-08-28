using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Category;

public sealed record CategoryCreatedEvent(Guid Id) : IDomainEvent;

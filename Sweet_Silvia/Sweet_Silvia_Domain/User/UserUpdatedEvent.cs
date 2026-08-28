using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.User;

public sealed record UserUpdatedEvent(Guid Id) : IDomainEvent;

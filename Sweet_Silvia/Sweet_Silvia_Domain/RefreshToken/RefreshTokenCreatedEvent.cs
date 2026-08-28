using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.RefreshToken;

public sealed record RefreshTokenCreatedEvent(Guid Id) : IDomainEvent;

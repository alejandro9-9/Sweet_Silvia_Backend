using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.PaymentMethod;

public sealed record PaymentMethodCreatedEvent(Guid Id) : IDomainEvent;

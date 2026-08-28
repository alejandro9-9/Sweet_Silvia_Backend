using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.PaymentMethod;

public sealed record PaymentMethodUpdatedEvent(Guid Id) : IDomainEvent;

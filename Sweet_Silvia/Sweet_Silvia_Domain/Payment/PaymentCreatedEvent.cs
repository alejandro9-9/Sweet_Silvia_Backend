using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Payment;

public sealed record PaymentCreatedEvent(Guid Id) : IDomainEvent;

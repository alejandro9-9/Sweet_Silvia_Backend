using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Payment;

public sealed record PaymentApprovedEvent(Guid Id) : IDomainEvent;

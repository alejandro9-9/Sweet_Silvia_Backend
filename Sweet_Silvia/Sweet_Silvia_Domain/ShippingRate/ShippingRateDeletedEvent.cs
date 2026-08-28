using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.ShippingRate;

public sealed record ShippingRateDeletedEvent(Guid Id) : IDomainEvent;

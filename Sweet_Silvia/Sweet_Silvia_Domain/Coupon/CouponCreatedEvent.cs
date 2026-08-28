using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Coupon;

public sealed record CouponCreatedEvent(Guid Id) : IDomainEvent;

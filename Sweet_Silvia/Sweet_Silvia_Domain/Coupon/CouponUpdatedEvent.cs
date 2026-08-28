using Sweet_Silvia_Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sweet_Silvia_Domain.Coupon;
public sealed record CouponUpdatedEvent(Guid Id) : IDomainEvent;
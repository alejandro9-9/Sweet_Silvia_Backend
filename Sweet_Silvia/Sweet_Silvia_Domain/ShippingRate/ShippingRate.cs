using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.ShippingRate;

public sealed class ShippingRate : Entity
{
    private ShippingRate() { }

    public ShippingRate(Guid id, Guid courierId, CourierType courierType, string country, Guid departmentId, Guid? provinceId, Guid? districtId, ShippingDestinationType destinationType, ShippingServiceType serviceType, Money cost, Money? freeShippingMinimumAmount, string? estimatedTime, bool isActive, DateTimeOffset createdAt)
        : base(id)
    {
        CourierId = courierId;
        CourierType = courierType;
        Country = country;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        DestinationType = destinationType;
        ServiceType = serviceType;
        Cost = cost;
        FreeShippingMinimumAmount = freeShippingMinimumAmount;
        EstimatedTime = estimatedTime;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid CourierId { get; private set; }
    public CourierType CourierType { get; private set; }
    public string Country { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Guid? ProvinceId { get; private set; }
    public Guid? DistrictId { get; private set; }
    public ShippingDestinationType DestinationType { get; private set; }
    public ShippingServiceType ServiceType { get; private set; }
    public Money Cost { get; private set; } = null!;
    public Money? FreeShippingMinimumAmount { get; private set; }
    public string? EstimatedTime { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static ShippingRate Create(Guid courierId, CourierType courierType, Guid departmentId, Guid? provinceId, Guid? districtId, ShippingDestinationType destinationType, ShippingServiceType serviceType, Money cost, Money? freeShippingMinimumAmount, string? estimatedTime)
    {
        Validate(courierId, courierType, departmentId, provinceId, districtId, destinationType, serviceType, cost, freeShippingMinimumAmount);

        var result = new ShippingRate(Guid.NewGuid(), courierId, courierType, "PE", departmentId, provinceId, districtId, destinationType, serviceType, cost, freeShippingMinimumAmount, estimatedTime?.Trim(), true, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new ShippingRateCreatedEvent(result.Id));
        return result;
    }

    public void Update(Guid courierId, CourierType courierType, Guid departmentId, Guid? provinceId, Guid? districtId, ShippingDestinationType destinationType, ShippingServiceType serviceType, Money cost, Money? freeShippingMinimumAmount, string? estimatedTime)
    {
        Validate(courierId, courierType, departmentId, provinceId, districtId, destinationType, serviceType, cost, freeShippingMinimumAmount);

        CourierId = courierId;
        CourierType = courierType;
        DepartmentId = departmentId;
        ProvinceId = provinceId;
        DistrictId = districtId;
        DestinationType = destinationType;
        ServiceType = serviceType;
        Cost = cost;
        FreeShippingMinimumAmount = freeShippingMinimumAmount;
        EstimatedTime = estimatedTime?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ShippingRateUpdatedEvent(Id));
    }

    public Money CalculateCost(Money orderSubtotal)
    {
        if (orderSubtotal is null)
        {
            throw new ArgumentNullException(nameof(orderSubtotal));
        }

        if (orderSubtotal.Currency != Cost.Currency)
        {
            throw new ArgumentException("Order subtotal should use the same currency as shipping cost.", nameof(orderSubtotal));
        }

        if (FreeShippingMinimumAmount is not null && orderSubtotal.Amount >= FreeShippingMinimumAmount.Amount)
        {
            return Money.Zero(Cost.Currency);
        }

        return Cost;
    }

    public bool AppliesTo(Guid departmentId, Guid? provinceId, Guid? districtId, ShippingDestinationType destinationType)
    {
        return IsActive &&
            DestinationType == destinationType &&
            DepartmentId == departmentId &&
            (!ProvinceId.HasValue || ProvinceId == provinceId) &&
            (!DistrictId.HasValue || DistrictId == districtId);
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ShippingRateDeletedEvent(Id));
    }

    private static void Validate(Guid courierId, CourierType courierType, Guid departmentId, Guid? provinceId, Guid? districtId, ShippingDestinationType destinationType, ShippingServiceType serviceType, Money cost, Money? freeShippingMinimumAmount)
    {
        if (courierId == Guid.Empty)
        {
            throw new ArgumentException("Courier id is required.", nameof(courierId));
        }

        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department id is required.", nameof(departmentId));
        }

        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("Province id is invalid.", nameof(provinceId));
        }

        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("District id is invalid.", nameof(districtId));
        }

        if (districtId.HasValue && !provinceId.HasValue)
        {
            throw new ArgumentException("Province id is required when district id is provided.", nameof(provinceId));
        }

        if (destinationType == ShippingDestinationType.Province && courierType != CourierType.Agency)
        {
            throw new ArgumentException("Province shipments should use an agency courier.", nameof(courierType));
        }

        if (destinationType == ShippingDestinationType.Province && serviceType != ShippingServiceType.Agency)
        {
            throw new ArgumentException("Province shipments should use agency service.", nameof(serviceType));
        }

        if (courierType == CourierType.OwnDelivery && serviceType == ShippingServiceType.Agency)
        {
            throw new ArgumentException("Own delivery cannot use agency service.", nameof(serviceType));
        }

        if (cost is null)
        {
            throw new ArgumentNullException(nameof(cost));
        }

        if (freeShippingMinimumAmount is not null && freeShippingMinimumAmount.Currency != cost.Currency)
        {
            throw new ArgumentException("Free shipping minimum amount should use the same currency as shipping cost.", nameof(freeShippingMinimumAmount));
        }
    }
}

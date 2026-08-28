using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Shipment;

public sealed class Shipment : Entity
{
    private Shipment() { }

    public Shipment(Guid id, Guid orderId, Guid courierId, ShipmentStatus status, string? trackingCode, string? externalShipmentCode, Money shippingCost, DateTimeOffset createdAt)
        : base(id)
    {
        OrderId = orderId;
        CourierId = courierId;
        Status = status;
        TrackingCode = trackingCode;
        ExternalShipmentCode = externalShipmentCode;
        ShippingCost = shippingCost;
        CreatedAt = createdAt;
    }

    public Guid OrderId { get; private set; }
    public Guid CourierId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? TrackingCode { get; private set; }
    public string? ExternalShipmentCode { get; private set; }
    public Money ShippingCost { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RegisteredExternallyAt { get; private set; }
    public DateTimeOffset? EstimatedDeliveryAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public string? Observation { get; private set; }

    public static Shipment Create(Guid orderId, Guid courierId, Money shippingCost)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (courierId == Guid.Empty)
        {
            throw new ArgumentException("Courier id is required.", nameof(courierId));
        }

        if (shippingCost is null)
        {
            throw new ArgumentNullException(nameof(shippingCost));
        }

        var result = new Shipment(Guid.NewGuid(), orderId, courierId, ShipmentStatus.Pending, null, null, shippingCost, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new ShipmentCreatedEvent(result.Id));
        return result;
    }

    public void RegisterExternalShipment(string trackingCode, string? externalShipmentCode, DateTimeOffset? estimatedDeliveryAt = null)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            throw new ArgumentException("Tracking code is required.", nameof(trackingCode));
        }

        Status = ShipmentStatus.Registered;
        TrackingCode = trackingCode.Trim();
        ExternalShipmentCode = externalShipmentCode?.Trim();
        EstimatedDeliveryAt = estimatedDeliveryAt;
        RegisteredExternallyAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ShipmentUpdatedEvent(Id));
    }

    public void ChangeStatus(ShipmentStatus status, string? observation = null)
    {
        Status = status;
        Observation = observation;

        if (status == ShipmentStatus.Delivered)
        {
            DeliveredAt = DateTimeOffset.UtcNow;
        }

        AddDomainEvent(new ShipmentUpdatedEvent(Id));
    }
}

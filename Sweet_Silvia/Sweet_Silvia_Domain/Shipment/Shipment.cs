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

    public static Shipment Create(Guid orderId, Guid courierId, Money shippingCost, string? observation = null)
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
        result.Observation = string.IsNullOrWhiteSpace(observation) ? null : observation.Trim();
        result.AddDomainEvent(new ShipmentCreatedEvent(result.Id));
        return result;
    }

    public void RegisterExternalShipment(string trackingCode, string? externalShipmentCode, DateTimeOffset? estimatedDeliveryAt = null)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
        {
            throw new ArgumentException("Tracking code is required.", nameof(trackingCode));
        }

        if (Status != ShipmentStatus.Registered && !IsValidTransition(Status, ShipmentStatus.Registered))
        {
            throw new InvalidOperationException($"Shipment cannot be registered from {Status}.");
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
        if (status == Status)
        {
            Observation = string.IsNullOrWhiteSpace(observation) ? null : observation.Trim();
            AddDomainEvent(new ShipmentUpdatedEvent(Id));
            return;
        }

        if (!IsValidTransition(Status, status))
        {
            throw new InvalidOperationException($"Shipment cannot change from {Status} to {status}.");
        }

        Status = status;
        Observation = observation;

        if (status == ShipmentStatus.Delivered)
        {
            DeliveredAt = DateTimeOffset.UtcNow;
        }

        AddDomainEvent(new ShipmentUpdatedEvent(Id));
    }

    private static bool IsValidTransition(ShipmentStatus current, ShipmentStatus next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            ShipmentStatus.Pending => next is ShipmentStatus.Coordinated or ShipmentStatus.Registered or ShipmentStatus.Cancelled or ShipmentStatus.Observed,
            ShipmentStatus.Coordinated => next is ShipmentStatus.Registered or ShipmentStatus.Cancelled or ShipmentStatus.Observed,
            ShipmentStatus.Registered => next is ShipmentStatus.InTransit or ShipmentStatus.Cancelled or ShipmentStatus.Observed,
            ShipmentStatus.InTransit => next is ShipmentStatus.Delivered or ShipmentStatus.Observed,
            ShipmentStatus.Observed => next is ShipmentStatus.Coordinated or ShipmentStatus.Registered or ShipmentStatus.Cancelled,
            _ => false
        };
    }
}

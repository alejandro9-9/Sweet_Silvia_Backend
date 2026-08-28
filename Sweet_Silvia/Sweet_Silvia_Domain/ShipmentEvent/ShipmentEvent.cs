using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shipment;

namespace Sweet_Silvia_Domain.ShipmentEvent;

public sealed class ShipmentEvent : Entity
{
    private ShipmentEvent() { }

    public ShipmentEvent(Guid id, Guid shipmentId, ShipmentStatus status, string description, string? location, DateTimeOffset eventDate)
        : base(id)
    {
        ShipmentId = shipmentId;
        Status = status;
        Description = description;
        Location = location;
        EventDate = eventDate;
    }

    public Guid ShipmentId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public DateTimeOffset EventDate { get; private set; }

    public static ShipmentEvent Create(Guid shipmentId, ShipmentStatus status, string description, string? location, DateTimeOffset eventDate)
    {
        if (shipmentId == Guid.Empty)
        {
            throw new ArgumentException("Shipment id is required.", nameof(shipmentId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Shipment event description is required.", nameof(description));
        }

        return new ShipmentEvent(Guid.NewGuid(), shipmentId, status, description.Trim(), location?.Trim(), eventDate);
    }
}

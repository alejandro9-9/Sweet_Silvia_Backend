using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Shipment;

namespace Sweet_Silvia_Application.ShipmentEvent;

public sealed record ShipmentEventResponse(Guid Id, Guid ShipmentId, ShipmentStatus Status, string Description, string? Location, DateTimeOffset EventDate);

public sealed record CreateShipmentEventCommand(Guid ShipmentId, ShipmentStatus Status, string Description, string? Location, DateTimeOffset EventDate) : ICommand<Guid>;

public sealed record GetShipmentEventsQuery(Guid ShipmentId) : IQuery<List<ShipmentEventResponse>>;

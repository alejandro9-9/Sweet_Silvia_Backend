using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Shipment;

namespace Sweet_Silvia_Application.Shipment;

public sealed record ShipmentResponse(Guid Id, Guid OrderId, Guid CourierId, ShipmentStatus Status, string? TrackingCode, string? ExternalShipmentCode, decimal ShippingCost, string Currency, DateTimeOffset CreatedAt, DateTimeOffset? RegisteredExternallyAt, DateTimeOffset? EstimatedDeliveryAt, DateTimeOffset? DeliveredAt, string? Observation);

public sealed record CreateShipmentCommand(Guid OrderId, Guid CourierId, decimal ShippingCost, string Currency = "PEN", string? Observation = null) : ICommand<Guid>;

public sealed record RegisterExternalShipmentCommand(Guid ShipmentId, string TrackingCode, string? ExternalShipmentCode, DateTimeOffset? EstimatedDeliveryAt) : ICommand;

public sealed record ChangeShipmentStatusCommand(Guid ShipmentId, ShipmentStatus Status, string? Observation, Guid ChangedByUserId) : ICommand;

public sealed record GetShipmentByIdQuery(Guid Id) : IQuery<ShipmentResponse>;

public sealed record GetShipmentsQuery(ShipmentStatus? Status = null) : IQuery<List<ShipmentResponse>>;

public sealed record GetShipmentsByOrderQuery(Guid OrderId) : IQuery<List<ShipmentResponse>>;

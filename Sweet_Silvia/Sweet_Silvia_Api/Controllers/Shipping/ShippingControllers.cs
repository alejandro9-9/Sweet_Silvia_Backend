using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Courier;
using Sweet_Silvia_Application.Order;
using Sweet_Silvia_Application.Shipment;
using Sweet_Silvia_Application.ShipmentEvent;
using Sweet_Silvia_Application.ShippingRate;
using Sweet_Silvia_Domain.Courier;
using Sweet_Silvia_Domain.Shared.Enums;
using Sweet_Silvia_Domain.Shipment;
using Sweet_Silvia_Domain.ShippingRate;

namespace Sweet_Silvia_Api.Controllers.Shipping;

[ApiController]
[Route("api/couriers")]
public sealed class CouriersController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetCouriersQuery(onlyActive), cancellationToken));

    [AllowAnonymous]
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCouriersQuery(true), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetCourierByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCourierRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateCourierCommand(request.Name, request.Type, request.RequiresExternalIntegration), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCourierRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateCourierCommand(id, request.Name, request.Type, request.RequiresExternalIntegration), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteCourierCommand(id), cancellationToken));
}

[ApiController]
[Route("api/shipping-rates")]
public sealed class ShippingRatesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? provinceId,
        [FromQuery] Guid? districtId,
        [FromQuery] ShippingDestinationType? destinationType,
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetShippingRatesQuery(departmentId, provinceId, districtId, destinationType, onlyActive), cancellationToken));

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(CalculateShippingCostRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CalculateShippingCostQuery(request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType, request.OrderSubtotal, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateShippingRateRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateShippingRateCommand(request.CourierId, request.CourierType, request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType, request.ServiceType, request.Cost, request.Currency, request.FreeShippingMinimumAmount, request.EstimatedTime), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateShippingRateRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateShippingRateCommand(id, request.CourierId, request.CourierType, request.DepartmentId, request.ProvinceId, request.DistrictId, request.DestinationType, request.ServiceType, request.Cost, request.Currency, request.FreeShippingMinimumAmount, request.EstimatedTime), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteShippingRateCommand(id), cancellationToken));
}

[ApiController]
[Route("api/shipments")]
public sealed class ShipmentsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ShipmentStatus? status, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetShipmentsQuery(status), cancellationToken));

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await sender.Send(new GetShipmentByIdQuery(id), cancellationToken);
        if (shipment.IsFailure)
        {
            return this.ToActionResult(shipment);
        }

        var access = await EnsureCanAccessOrder(shipment.Value.OrderId, cancellationToken);
        return access ?? this.ToActionResult(shipment);
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var access = await EnsureCanAccessOrder(orderId, cancellationToken);
        if (access is not null)
        {
            return access;
        }

        return this.ToActionResult(await sender.Send(new GetShipmentsByOrderQuery(orderId), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateShipmentRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateShipmentCommand(request.OrderId, request.CourierId, request.ShippingCost, request.Currency), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPatch("{id:guid}/register-external")]
    public async Task<IActionResult> RegisterExternal(Guid id, RegisterExternalShipmentRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new RegisterExternalShipmentCommand(id, request.TrackingCode, request.ExternalShipmentCode, request.EstimatedDeliveryAt), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeShipmentStatusRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ChangeShipmentStatusCommand(id, request.Status, request.Observation), cancellationToken));

    private async Task<IActionResult?> EnsureCanAccessOrder(Guid orderId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(ApiRoles.Customer))
        {
            return null;
        }

        var order = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        if (order.IsFailure)
        {
            return this.ToActionResult(order);
        }

        return order.Value.UserId == User.GetUserId() ? null : Forbid();
    }
}

[ApiController]
[Route("api/shipment-events")]
public sealed class ShipmentEventsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("shipment/{shipmentId:guid}")]
    public async Task<IActionResult> GetByShipment(Guid shipmentId, CancellationToken cancellationToken)
    {
        var shipment = await sender.Send(new GetShipmentByIdQuery(shipmentId), cancellationToken);
        if (shipment.IsFailure)
        {
            return this.ToActionResult(shipment);
        }

        if (User.IsInRole(ApiRoles.Customer))
        {
            var order = await sender.Send(new GetOrderByIdQuery(shipment.Value.OrderId), cancellationToken);
            if (order.IsFailure)
            {
                return this.ToActionResult(order);
            }

            if (order.Value.UserId != User.GetUserId())
            {
                return Forbid();
            }
        }

        return this.ToActionResult(await sender.Send(new GetShipmentEventsQuery(shipmentId), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateShipmentEventRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateShipmentEventCommand(request.ShipmentId, request.Status, request.Description, request.Location, request.EventDate), cancellationToken));
}

public sealed record CreateCourierRequest(string Name, CourierType Type, bool RequiresExternalIntegration);
public sealed record UpdateCourierRequest(string Name, CourierType Type, bool RequiresExternalIntegration);
public sealed record CalculateShippingCostRequest(Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, decimal OrderSubtotal, string Currency = "PEN");
public sealed record CreateShippingRateRequest(Guid CourierId, CourierType CourierType, Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, ShippingServiceType ServiceType, decimal Cost, string Currency = "PEN", decimal? FreeShippingMinimumAmount = null, string? EstimatedTime = null);
public sealed record UpdateShippingRateRequest(Guid CourierId, CourierType CourierType, Guid DepartmentId, Guid? ProvinceId, Guid? DistrictId, ShippingDestinationType DestinationType, ShippingServiceType ServiceType, decimal Cost, string Currency = "PEN", decimal? FreeShippingMinimumAmount = null, string? EstimatedTime = null);
public sealed record CreateShipmentRequest(Guid OrderId, Guid CourierId, decimal ShippingCost, string Currency = "PEN");
public sealed record RegisterExternalShipmentRequest(string TrackingCode, string? ExternalShipmentCode, DateTimeOffset? EstimatedDeliveryAt);
public sealed record ChangeShipmentStatusRequest(ShipmentStatus Status, string? Observation);
public sealed record CreateShipmentEventRequest(Guid ShipmentId, ShipmentStatus Status, string Description, string? Location, DateTimeOffset EventDate);

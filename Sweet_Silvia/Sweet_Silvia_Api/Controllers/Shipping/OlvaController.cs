using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Olva;
using Sweet_Silvia_Application.Order;
using Sweet_Silvia_Application.Shipment;

namespace Sweet_Silvia_Api.Controllers.Shipping;

[ApiController]
[Route("api/shipping/olva")]
[Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
public sealed class OlvaController(ISender sender) : ControllerBase
{
    [HttpGet("validate")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new ValidateOlvaApiKeyQuery(), cancellationToken));
    }

    [HttpGet("agencies")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAgencies(
        [FromQuery] string? q,
        [FromQuery] string? department,
        [FromQuery] string? province,
        [FromQuery] string? district,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return this.ToActionResult(await sender.Send(new GetOlvaAgenciesQuery(q, department, province, district, page, pageSize), cancellationToken));
    }

    [HttpGet("cached-locations/departments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCachedDepartments(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetCachedOlvaDepartmentsQuery(), cancellationToken));
    }

    [HttpGet("cached-locations/departments/{department}/provinces")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCachedProvinces(string department, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetCachedOlvaProvincesQuery(department), cancellationToken));
    }

    [HttpGet("cached-locations/departments/{department}/provinces/{province}/districts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCachedDistricts(string department, string province, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetCachedOlvaDistrictsQuery(department, province), cancellationToken));
    }

    [HttpPost("agencies/sync")]
    public async Task<IActionResult> SyncAgencies(SyncOlvaAgenciesRequest request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(
            new SyncOlvaAgenciesCommand(request.Q, request.Department, request.Province, request.District),
            cancellationToken));
    }

    [HttpGet("locations/departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetOlvaDepartmentsQuery(), cancellationToken));
    }

    [HttpGet("locations/departments/{department}/provinces")]
    public async Task<IActionResult> GetProvinces(string department, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetOlvaProvincesQuery(department), cancellationToken));
    }

    [HttpGet("locations/departments/{department}/provinces/{province}/districts")]
    public async Task<IActionResult> GetDistricts(string department, string province, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new GetOlvaDistrictsQuery(department, province), cancellationToken));
    }

    [HttpPost("quote")]
    public async Task<IActionResult> Quote(QuoteOlvaShipmentRequest request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(
            new QuoteOlvaShipmentQuery(
                request.Origin,
                request.Destination,
                request.DeliveryType,
                request.ShipmentType,
                request.Weight,
                request.PartnerRate),
            cancellationToken));
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track(TrackOlvaShipmentRequest request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(new TrackOlvaShipmentQuery(request.OrderNumber, request.OrderCode), cancellationToken));
    }
}

public sealed record QuoteOlvaShipmentRequest(
    string Origin,
    string Destination,
    string? DeliveryType = "O",
    int? ShipmentType = null,
    decimal? Weight = 1,
    bool? PartnerRate = null);

public sealed record TrackOlvaShipmentRequest(string OrderNumber, string? OrderCode = null);

public sealed record SyncOlvaAgenciesRequest(
    string? Q = null,
    string? Department = null,
    string? Province = null,
    string? District = null);

[ApiController]
[Route("api/shipping/olva/customer")]
[Authorize(Roles = ApiRoles.Customer)]
public sealed class CustomerOlvaController(ISender sender) : ControllerBase
{
    [HttpPost("orders/{orderId:guid}/track")]
    public async Task<IActionResult> TrackOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        if (order.IsFailure)
        {
            return this.ToActionResult(order);
        }

        if (order.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var shipments = await sender.Send(new GetShipmentsByOrderQuery(orderId), cancellationToken);
        if (shipments.IsFailure)
        {
            return this.ToActionResult(shipments);
        }

        var shipment = shipments.Value.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.TrackingCode));
        if (shipment is null)
        {
            return NotFound(new
            {
                code = "Olva.TrackingUnavailable",
                message = "El pedido todavía no tiene un código de seguimiento de Olva registrado."
            });
        }

        return this.ToActionResult(await sender.Send(
            new TrackOlvaShipmentQuery(shipment.TrackingCode!, shipment.ExternalShipmentCode),
            cancellationToken));
    }
}

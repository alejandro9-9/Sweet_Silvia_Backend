using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Address;
using Sweet_Silvia_Application.Order;
using Sweet_Silvia_Application.OrderItem;
using Sweet_Silvia_Application.OrderStatusHistory;
using Sweet_Silvia_Domain.Order;

namespace Sweet_Silvia_Api.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetOrdersQuery(status), cancellationToken));

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetOrdersByUserQuery(User.GetUserId()), cancellationToken));

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (result.IsSuccess && User.IsInRole(ApiRoles.Customer) && result.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToActionResult(result);
    }

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var address = await sender.Send(new GetAddressByIdQuery(request.AddressId), cancellationToken);
        if (address.IsFailure)
        {
            return this.ToActionResult(address);
        }

        if (address.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToCreatedResult(await sender.Send(new CreateOrderCommand(User.GetUserId(), request.AddressId, request.ShippingRateId, request.CouponId), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeOrderStatusRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ChangeOrderStatusCommand(id, request.Status, User.GetUserId(), request.Observation), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ChangeOrderStatusCommand(id, OrderStatus.Cancelled, User.GetUserId(), "Cancelled by administrator."), cancellationToken));
}

[ApiController]
[Route("api/order-items")]
public sealed class OrderItemsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        if (order.IsFailure)
        {
            return this.ToActionResult(order);
        }

        if (User.IsInRole(ApiRoles.Customer) && order.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToActionResult(await sender.Send(new GetOrderItemsByOrderQuery(orderId), cancellationToken));
    }
}

[ApiController]
[Route("api/order-status-history")]
public sealed class OrderStatusHistoryController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetOrderStatusHistoryQuery(orderId), cancellationToken));
}

public sealed record CreateOrderRequest(Guid AddressId, Guid ShippingRateId, Guid? CouponId = null);
public sealed record ChangeOrderStatusRequest(OrderStatus Status, string? Observation);

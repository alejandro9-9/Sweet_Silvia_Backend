using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.StockMovement;
using Sweet_Silvia_Domain.StockMovement;

namespace Sweet_Silvia_Api.Controllers.Stock;

[ApiController]
[Route("api/stock-movements")]
public sealed class StockMovementsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetStockMovementsQuery(), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("product-variant/{productVariantId:guid}")]
    public async Task<IActionResult> GetByProductVariant(Guid productVariantId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetStockMovementsByProductVariantQuery(productVariantId), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetStockMovementsByOrderQuery(orderId), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost("manual-adjustment")]
    public async Task<IActionResult> CreateManualAdjustment(CreateStockMovementRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreateStockMovementCommand(request.ProductVariantId, request.OrderId, StockMovementType.ManualAdjustment, request.Quantity, request.PreviousStock, request.NewStock, request.Observation), cancellationToken));
}

public sealed record CreateStockMovementRequest(Guid ProductVariantId, Guid? OrderId, int Quantity, int PreviousStock, int NewStock, string? Observation);

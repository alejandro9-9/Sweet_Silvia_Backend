using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Order;
using Sweet_Silvia_Application.Payment;
using Sweet_Silvia_Application.PaymentMethod;
using Sweet_Silvia_Application.PaymentReceipt;
using Sweet_Silvia_Application.PaymentReview;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.PaymentMethod;

namespace Sweet_Silvia_Api.Controllers.Payments;

[ApiController]
[Route("api/payment-methods")]
public sealed class PaymentMethodsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(new GetPaymentMethodsQuery(onlyActive), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPaymentMethodByIdQuery(id), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentMethodRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new CreatePaymentMethodCommand(request.Name, request.Type, request.RequiresManualVerification, request.RequiresExternalIntegration), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdatePaymentMethodCommand(id, request.Name, request.Type, request.RequiresManualVerification, request.RequiresExternalIntegration), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeletePaymentMethodCommand(id), cancellationToken));
}

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaymentStatus? status, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPaymentsQuery(status), cancellationToken));

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await sender.Send(new GetPaymentByIdQuery(id), cancellationToken);
        if (payment.IsFailure)
        {
            return this.ToActionResult(payment);
        }

        var access = await EnsureCanAccessOrder(payment.Value.OrderId, cancellationToken);
        return access ?? this.ToActionResult(payment);
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

        return this.ToActionResult(await sender.Send(new GetPaymentsByOrderQuery(orderId), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var access = await EnsureCanAccessOrder(request.OrderId, cancellationToken);
        if (access is not null)
        {
            return access;
        }

        return this.ToCreatedResult(await sender.Send(new CreatePendingPaymentCommand(request.OrderId, request.PaymentMethodId, request.Amount, request.Currency), cancellationToken));
    }

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
[Route("api/payment-receipts")]
public sealed class PaymentReceiptsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost]
    public async Task<IActionResult> Upload(UploadPaymentReceiptRequest request, CancellationToken cancellationToken)
    {
        var payment = await sender.Send(new GetPaymentByIdQuery(request.PaymentId), cancellationToken);
        if (payment.IsFailure)
        {
            return this.ToActionResult(payment);
        }

        var order = await sender.Send(new GetOrderByIdQuery(payment.Value.OrderId), cancellationToken);
        if (order.IsFailure)
        {
            return this.ToActionResult(order);
        }

        if (order.Value.UserId != User.GetUserId())
        {
            return Forbid();
        }

        return this.ToCreatedResult(await sender.Send(new UploadPaymentReceiptCommand(request.PaymentId, request.FileUrl, request.OperationCode, request.DeclaredAmount, request.Currency), cancellationToken));
    }

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetByPayment(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await sender.Send(new GetPaymentByIdQuery(paymentId), cancellationToken);
        if (payment.IsFailure)
        {
            return this.ToActionResult(payment);
        }

        if (User.IsInRole(ApiRoles.Customer))
        {
            var order = await sender.Send(new GetOrderByIdQuery(payment.Value.OrderId), cancellationToken);
            if (order.IsFailure)
            {
                return this.ToActionResult(order);
            }

            if (order.Value.UserId != User.GetUserId())
            {
                return Forbid();
            }
        }

        return this.ToActionResult(await sender.Send(new GetPaymentReceiptsByPaymentQuery(paymentId), cancellationToken));
    }
}

[ApiController]
[Route("api/payment-reviews")]
public sealed class PaymentReviewsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPaymentReviewsQuery(), cancellationToken));

    [Authorize(Roles = ApiRoles.AdministratorOrAssistant)]
    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetByPayment(Guid paymentId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPaymentReviewsByPaymentQuery(paymentId), cancellationToken));

    [Authorize(Roles = ApiRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Review(ReviewPaymentRequest request, CancellationToken cancellationToken)
        => this.ToCreatedResult(await sender.Send(new ReviewPaymentCommand(request.PaymentId, User.GetUserId(), request.Result, request.Observation), cancellationToken));
}

public sealed record CreatePaymentMethodRequest(string Name, PaymentMethodType Type, bool RequiresManualVerification, bool RequiresExternalIntegration);
public sealed record UpdatePaymentMethodRequest(string Name, PaymentMethodType Type, bool RequiresManualVerification, bool RequiresExternalIntegration);
public sealed record CreatePaymentRequest(Guid OrderId, Guid PaymentMethodId, decimal Amount, string Currency = "PEN");
public sealed record UploadPaymentReceiptRequest(Guid PaymentId, string FileUrl, string? OperationCode, decimal? DeclaredAmount, string Currency = "PEN");
public sealed record ReviewPaymentRequest(Guid PaymentId, PaymentReviewResult Result, string? Observation);

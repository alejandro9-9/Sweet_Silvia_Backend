using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Abstractions.Storage;
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
public sealed class PaymentReceiptsController(ISender sender, IFileStorageService fileStorage) : ControllerBase
{
    private const long MaximumReceiptSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedReceiptExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".pdf"
    };

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

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost("upload-file")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumReceiptSizeBytes)]
    public async Task<IActionResult> UploadFile([FromForm] UploadPaymentReceiptFileRequest request, CancellationToken cancellationToken)
    {
        var validationError = ValidateFile(request.File, AllowedReceiptExtensions, MaximumReceiptSizeBytes);
        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

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

        var storedFile = await fileStorage.SaveAsync(
            new FileStorageUpload(
                "payment-receipts",
                request.File.FileName,
                request.File.ContentType,
                request.File.OpenReadStream()),
            cancellationToken);

        var result = await sender.Send(
            new UploadPaymentReceiptCommand(
                request.PaymentId,
                storedFile.Url,
                request.OperationCode,
                request.DeclaredAmount,
                request.Currency),
            cancellationToken);

        return result.IsSuccess
            ? Ok(new { id = result.Value, url = storedFile.Url, key = storedFile.Key })
            : BadRequest(result.Error);
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

    private static string? ValidateFile(IFormFile file, HashSet<string> allowedExtensions, long maximumSizeBytes)
    {
        if (file is null || file.Length == 0)
        {
            return "File is required.";
        }

        if (file.Length > maximumSizeBytes)
        {
            return $"File should not exceed {maximumSizeBytes / 1024 / 1024} MB.";
        }

        var extension = Path.GetExtension(file.FileName);
        return allowedExtensions.Contains(extension)
            ? null
            : $"File extension '{extension}' is not allowed.";
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

public sealed class UploadPaymentReceiptFileRequest
{
    public Guid PaymentId { get; init; }
    public string? OperationCode { get; init; }
    public decimal? DeclaredAmount { get; init; }
    public string Currency { get; init; } = "PEN";
    public IFormFile File { get; init; } = null!;
}

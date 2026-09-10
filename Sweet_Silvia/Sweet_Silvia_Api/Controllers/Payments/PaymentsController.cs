using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Security;
using Sweet_Silvia_Application.Abstractions.Storage;
using Sweet_Silvia_Application.Izipay;
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
public sealed class PaymentMethodsController(ISender sender, IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPaymentMethodsQuery(onlyActive || !User.IsInRole(ApiRoles.Administrator)), cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        if (!User.IsInRole(ApiRoles.Administrator) && !IsIzipayReady())
        {
            result.Value.RemoveAll(method =>
                method.Type == PaymentMethodType.Gateway &&
                method.RequiresExternalIntegration &&
                string.Equals(method.Name, "Izipay", StringComparison.OrdinalIgnoreCase));
        }

        return Ok(result.Value);
    }

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

    private bool IsIzipayReady()
    {
        var options = configuration.GetSection("Izipay");
        var notificationUrl = options["NotificationUrl"];
        var termsUrl = options["TermsAndConditionsUrl"];

        return bool.TryParse(options["Enabled"], out var enabled) &&
               enabled &&
               HasConfiguredValue(options["MerchantCode"]) &&
               HasConfiguredValue(options["Username"]) &&
               HasConfiguredValue(options["Password"]) &&
               HasConfiguredValue(options["PublicKey"]) &&
               HasConfiguredValue(options["HashKey"]) &&
               IsPublicHttpsUrl(notificationUrl) &&
               IsPublicHttpsUrl(termsUrl);
    }

    private static bool HasConfiguredValue(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("your-", StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           uri.Scheme == Uri.UriSchemeHttps &&
           !uri.IsLoopback &&
           !uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) &&
           !uri.Host.Contains("your-", StringComparison.OrdinalIgnoreCase);
}

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(ISender sender, IConfiguration configuration) : ControllerBase
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

    [Authorize(Roles = ApiRoles.Customer)]
    [HttpPost("izipay/payment-link")]
    public async Task<IActionResult> GenerateIzipayPaymentLink(GenerateIzipayPaymentLinkRequest request, CancellationToken cancellationToken)
    {
        if (!IsIzipayReady())
        {
            return Conflict(new
            {
                code = "Izipay.NotAvailable",
                message = "Izipay estara disponible cuando la tienda este desplegada y las URLs publicas hayan sido configuradas."
            });
        }

        var access = await EnsureCanAccessOrder(request.OrderId, cancellationToken);
        if (access is not null)
        {
            return access;
        }

        return this.ToActionResult(await sender.Send(
            new GenerateIzipayPaymentLinkCommand(
                request.OrderId,
                request.PaymentMethodId,
                request.ProductDescription,
                request.ExpirationDate,
                request.PayMethod,
                request.Billing,
                request.Shipping),
            cancellationToken));
    }

    private bool IsIzipayReady()
    {
        var options = configuration.GetSection("Izipay");
        return bool.TryParse(options["Enabled"], out var enabled) &&
               enabled &&
               HasConfiguredValue(options["MerchantCode"]) &&
               HasConfiguredValue(options["Username"]) &&
               HasConfiguredValue(options["Password"]) &&
               HasConfiguredValue(options["PublicKey"]) &&
               HasConfiguredValue(options["HashKey"]) &&
               IsPublicHttpsUrl(options["NotificationUrl"]) &&
               IsPublicHttpsUrl(options["TermsAndConditionsUrl"]);
    }

    private static bool HasConfiguredValue(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("your-", StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           uri.Scheme == Uri.UriSchemeHttps &&
           !uri.IsLoopback &&
           !uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) &&
           !uri.Host.Contains("your-", StringComparison.OrdinalIgnoreCase);

    [AllowAnonymous]
    [HttpPost("izipay/notifications")]
    public async Task<IActionResult> ReceiveIzipayNotification(
        [FromHeader(Name = "transactionId")] string? transactionId,
        [FromHeader(Name = "Signature")] string? signatureHeader,
        IzipayNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsIzipayReady())
        {
            return Conflict(new
            {
                code = "Izipay.NotAvailable",
                message = "Izipay estara disponible cuando la tienda este desplegada y las URLs publicas hayan sido configuradas."
            });
        }

        return this.ToActionResult(await sender.Send(
            new ProcessIzipayNotificationCommand(
                transactionId ?? string.Empty,
                request.TransactionId,
                request.Code,
                request.Message,
                request.PayloadHttp,
                string.IsNullOrWhiteSpace(signatureHeader) ? request.Signature : signatureHeader,
                request.Response is null
                    ? null
                    : new IzipayNotificationResponseData(request.Response.Order)),
            cancellationToken));
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
    [HttpPost("upload-file")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumReceiptSizeBytes)]
    public async Task<IActionResult> UploadFile([FromForm] UploadPaymentReceiptFileRequest request, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFileAsync(request.File, AllowedReceiptExtensions, MaximumReceiptSizeBytes, cancellationToken);
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

        await using var fileStream = request.File.OpenReadStream();
        var storedFile = await fileStorage.SaveAsync(
            new FileStorageUpload(
                "payment-receipts",
                request.File.FileName,
                request.File.ContentType,
                fileStream,
                false),
            cancellationToken);

        try
        {
            var result = await sender.Send(
                new UploadPaymentReceiptCommand(
                    request.PaymentId,
                    storedFile.Key,
                    request.OperationCode,
                    request.DeclaredAmount,
                    request.Currency),
                cancellationToken);

            if (result.IsFailure)
            {
                await fileStorage.DeleteAsync(storedFile.Key, cancellationToken);
                return BadRequest(result.Error);
            }

            return Ok(new { id = result.Value, url = storedFile.Url, key = storedFile.Key });
        }
        catch
        {
            await fileStorage.DeleteAsync(storedFile.Key, cancellationToken);
            throw;
        }
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

    [Authorize(Roles = ApiRoles.AnyAuthenticated)]
    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await sender.Send(new GetPaymentReceiptByIdQuery(id), cancellationToken);
        if (receipt.IsFailure)
        {
            return this.ToActionResult(receipt);
        }

        var payment = await sender.Send(new GetPaymentByIdQuery(receipt.Value.PaymentId), cancellationToken);
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

        var file = await fileStorage.OpenPrivateReadAsync(receipt.Value.FileUrl, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        return File(file.Content, file.ContentType, enableRangeProcessing: true);
    }

    private static async Task<string?> ValidateFileAsync(IFormFile file, HashSet<string> allowedExtensions, long maximumSizeBytes, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return "File is required.";
        }

        if (file.Length > maximumSizeBytes)
        {
            return $"File should not exceed {maximumSizeBytes / 1024 / 1024} MB.";
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return $"File extension '{extension}' is not allowed.";
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        var validSignature = extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => read >= 12 && header[..4].SequenceEqual("RIFF"u8.ToArray()) && header[8..12].SequenceEqual("WEBP"u8.ToArray()),
            ".pdf" => read >= 4 && header[..4].SequenceEqual("%PDF"u8.ToArray()),
            _ => false
        };

        return validSignature ? null : "The file content does not match its extension.";
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
public sealed class GenerateIzipayPaymentLinkRequest
{
    public Guid OrderId { get; init; }
    public Guid PaymentMethodId { get; init; }
    public string? ProductDescription { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
    public string PayMethod { get; init; } = "CARD";
    public IzipayPaymentLinkContact Billing { get; init; } = null!;
    public IzipayPaymentLinkContact? Shipping { get; init; }
}

public sealed class IzipayNotificationRequest
{
    public string Code { get; init; } = string.Empty;
    public string? Message { get; init; }
    public IzipayNotificationResponseRequest? Response { get; init; }
    public string? PayloadHttp { get; init; }
    public string? Signature { get; init; }
    public string? TransactionId { get; init; }
}

public sealed class IzipayNotificationResponseRequest
{
    public List<IzipayNotificationOrder> Order { get; init; } = [];
}

public sealed record ReviewPaymentRequest(Guid PaymentId, PaymentReviewResult Result, string? Observation);

public sealed class UploadPaymentReceiptFileRequest
{
    public Guid PaymentId { get; init; }
    public string? OperationCode { get; init; }
    public decimal? DeclaredAmount { get; init; }
    public string Currency { get; init; } = "PEN";
    public IFormFile File { get; init; } = null!;
}

using System.Globalization;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.Payment;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Coupon;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.PaymentMethod;
using Sweet_Silvia_Domain.StockMovement;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;

namespace Sweet_Silvia_Application.Izipay;

internal sealed class GenerateIzipayPaymentLinkCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<PaymentMethodEntity> paymentMethods,
    IIzipayPaymentGateway izipayPaymentGateway,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GenerateIzipayPaymentLinkCommand, IzipayPaymentLinkResponse>
{
    private static readonly HashSet<string> AllowedPayMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "CARD",
        "QR",
        "YAPE_CODE",
        "PAGO_PUSH"
    };

    public async Task<Result<IzipayPaymentLinkResponse>> Handle(GenerateIzipayPaymentLinkCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var paymentMethod = await paymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null || !paymentMethod.IsActive)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.NotFound("PaymentMethod", request.PaymentMethodId));
        }

        if (paymentMethod.Type != PaymentMethodType.Gateway || !paymentMethod.RequiresExternalIntegration)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "PaymentMethod.InvalidGateway",
                "The selected payment method is not configured as an external gateway."));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "A payment link can only be generated for an order awaiting payment."));
        }

        if (request.Billing is null)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Izipay.BillingRequired",
                "Billing information is required to generate an Izipay payment link."));
        }

        var payMethod = string.IsNullOrWhiteSpace(request.PayMethod) ? "CARD" : request.PayMethod.Trim().ToUpperInvariant();
        if (!AllowedPayMethods.Contains(payMethod))
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Izipay.InvalidPayMethod",
                "Izipay pay method should be CARD, QR, YAPE_CODE or PAGO_PUSH."));
        }

        var now = DateTimeOffset.UtcNow;
        var pendingGatewayPayments = payments.Query()
            .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.PendingGateway)
            .ToList();

        var expiredPayments = pendingGatewayPayments
            .Where(payment => payment.GatewayExpiresAt.HasValue && payment.GatewayExpiresAt.Value <= now)
            .ToList();

        foreach (var expiredPayment in expiredPayments)
        {
            expiredPayment.ExpireGateway();
        }

        if (expiredPayments.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (payments.Query().Any(payment =>
                payment.OrderId == order.Id &&
                payment.Status == PaymentStatus.PendingGateway &&
                (!payment.GatewayExpiresAt.HasValue || payment.GatewayExpiresAt.Value > now)))
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Payment.ActiveGatewayPaymentExists",
                "This order already has an active Izipay payment."));
        }

        var contactError = IzipayValidation.ValidateContact(request.Billing);
        if (contactError is not null)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict("Izipay.InvalidContact", contactError));
        }

        var shippingContact = request.Shipping ?? request.Billing;
        contactError = IzipayValidation.ValidateContact(shippingContact);
        if (contactError is not null)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict("Izipay.InvalidContact", contactError));
        }

        var expirationDate = IzipayValidation.NormalizeExpiration(request.ExpirationDate ?? DateTimeOffset.UtcNow.AddHours(24));
        if (expirationDate <= DateTimeOffset.UtcNow || expirationDate > DateTimeOffset.UtcNow.AddDays(180))
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Izipay.InvalidExpiration",
                "Izipay payment link expiration should be between now and 180 days."));
        }

        var transactionId = Guid.NewGuid().ToString("N");
        var gatewayResult = await izipayPaymentGateway.GeneratePaymentLinkAsync(
            new IzipayPaymentLinkGatewayRequest(
                transactionId,
                string.IsNullOrWhiteSpace(request.ProductDescription)
                    ? $"Pedido Sweet Silvia {order.Id:N}"
                    : request.ProductDescription.Trim(),
                order.Total.Amount,
                order.Total.Currency,
                expirationDate,
                "INDIVIDUAL",
                payMethod,
                request.Billing,
                shippingContact),
            cancellationToken);

        if (gatewayResult.IsFailure)
        {
            return Result.Failure<IzipayPaymentLinkResponse>(gatewayResult.Error);
        }

        if (!decimal.TryParse(gatewayResult.Value.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var gatewayAmount) ||
            gatewayAmount != order.Total.Amount ||
            !string.Equals(gatewayResult.Value.Currency, order.Total.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Izipay.AmountMismatch",
                "Izipay returned an amount or currency different from the order."));
        }

        var payment = PaymentEntity.CreateGatewayPending(
            order.Id,
            paymentMethod.Id,
            order.Total,
            gatewayResult.Value.TransactionId,
            gatewayResult.Value.PaymentLinkId,
            expirationDate,
            gatewayResult.Value.State);

        payments.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new IzipayPaymentLinkResponse(
            payment.Id,
            gatewayResult.Value.TransactionId,
            gatewayResult.Value.PaymentLinkId,
            gatewayResult.Value.PaymentUrl,
            gatewayResult.Value.Amount,
            gatewayResult.Value.Currency,
            gatewayResult.Value.State);
    }
}

internal sealed class ProcessIzipayNotificationCommandHandler(
    IIzipaySignatureValidator signatureValidator,
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IRepository<Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory> histories,
    IRepository<CouponEntity> coupons,
    IRepository<CouponUsageEntity> couponUsages,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProcessIzipayNotificationCommand, IzipayNotificationResult>
{
    public async Task<Result<IzipayNotificationResult>> Handle(ProcessIzipayNotificationCommand request, CancellationToken cancellationToken)
    {
        var headerTransactionId = request.HeaderTransactionId?.Trim();
        if (string.IsNullOrWhiteSpace(headerTransactionId))
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.TransactionIdMissing",
                "Izipay transactionId header is required."));
        }

        if (!string.IsNullOrWhiteSpace(request.BodyTransactionId) &&
            !string.Equals(headerTransactionId, request.BodyTransactionId.Trim(), StringComparison.Ordinal))
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.TransactionIdMismatch",
                "Izipay transactionId header and body do not match."));
        }

        var isCommunicationError = string.Equals(request.Code, "021", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(request.Code, "COMMUNICATION_ERROR", StringComparison.OrdinalIgnoreCase);
        SignedIzipayNotification? signedNotification = null;
        if (!isCommunicationError &&
            (string.IsNullOrWhiteSpace(request.PayloadHttp) ||
             string.IsNullOrWhiteSpace(request.Signature) ||
             !signatureValidator.IsValid(request.PayloadHttp, request.Signature)))
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.InvalidSignature",
                "Izipay notification signature is invalid."));
        }

        if (!isCommunicationError)
        {
            try
            {
                signedNotification = JsonSerializer.Deserialize<SignedIzipayNotification>(request.PayloadHttp!, IzipayNotificationJsonOptions.Instance);
            }
            catch (JsonException)
            {
                signedNotification = null;
            }

            if (signedNotification is null || string.IsNullOrWhiteSpace(signedNotification.Code))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.InvalidPayload",
                    "Izipay signed payload could not be read."));
            }

            if (!string.Equals(signedNotification.TransactionId?.Trim(), headerTransactionId, StringComparison.Ordinal))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.TransactionIdMismatch",
                    "Izipay signed payload and transactionId header do not match."));
            }

            if (!string.Equals(signedNotification.Code.Trim(), request.Code.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.CodeMismatch",
                    "Izipay notification code is not consistent with its signed payload."));
            }
        }

        var payment = payments.Query()
            .FirstOrDefault(item => item.GatewayTransactionId == headerTransactionId);
        if (payment is null)
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.NotFound("Payment"));
        }

        if (payment.Status is PaymentStatus.Approved or PaymentStatus.Rejected)
        {
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        if (payment.Status != PaymentStatus.PendingGateway)
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.PaymentNotPending",
                "The payment is no longer pending gateway confirmation."));
        }

        if (isCommunicationError)
        {
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        var signedCode = signedNotification?.Code ?? request.Code;
        var gatewayOrder = signedNotification?.Response?.Order?.FirstOrDefault();
        if (!string.Equals(signedCode, "00", StringComparison.OrdinalIgnoreCase) &&
            payment.GatewayExpiresAt.HasValue &&
            payment.GatewayExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            payment.ExpireGateway();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        if (string.Equals(signedCode, "00", StringComparison.OrdinalIgnoreCase))
        {
            if (gatewayOrder is null ||
                !decimal.TryParse(gatewayOrder.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.InvalidAmount",
                    "Izipay notification did not contain a valid amount."));
            }

            if (amount != payment.Amount.Amount ||
                !string.Equals(gatewayOrder.Currency, payment.Amount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.AmountMismatch",
                    "Izipay notification amount does not match the payment amount."));
            }

            if (!string.IsNullOrWhiteSpace(payment.TransactionCode) &&
                !string.IsNullOrWhiteSpace(gatewayOrder.PaymentLinkId) &&
                !string.Equals(payment.TransactionCode, gatewayOrder.PaymentLinkId, StringComparison.Ordinal))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.PaymentLinkMismatch",
                    "Izipay notification does not belong to the stored payment link."));
            }

            var settlement = await PaymentSettlementService.Approve(
                payment,
                null,
                gatewayOrder.CodeAuth,
                $"Izipay payment approved ({signedCode}).",
                false,
                orders,
                orderItems,
                variants,
                stockMovements,
                histories,
                coupons,
                couponUsages,
                cancellationToken);
            if (settlement.IsFailure)
            {
                if (settlement.Error.Code == "Stock.NotEnough")
                {
                    // Do not reject a gateway payment after Izipay approved it.
                    // Keep it pending for reconciliation/refund instead.
                    payment.MarkGatewaySettlementPending(
                        "Izipay aprobo el pago, pero el stock requiere conciliacion manual: " + settlement.Error.Message);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    return new IzipayNotificationResult(payment.Id, payment.Status);
                }

                return Result.Failure<IzipayNotificationResult>(settlement.Error);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        payment.RejectFromGateway($"Izipay payment rejected ({signedCode}): {signedNotification?.Message?.Trim() ?? request.Message?.Trim()}");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new IzipayNotificationResult(payment.Id, payment.Status);
    }

}

internal sealed class SignedIzipayNotification
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public SignedIzipayResponse? Response { get; set; }
    public string? TransactionId { get; set; }
}

internal sealed class SignedIzipayResponse
{
    public List<SignedIzipayOrder> Order { get; set; } = [];
}

internal sealed class SignedIzipayOrder
{
    public string? CodeAuth { get; set; }
    public string? Currency { get; set; }
    public string? Amount { get; set; }
    public string? PaymentLinkId { get; set; }
}

internal static class IzipayNotificationJsonOptions
{
    public static readonly JsonSerializerOptions Instance = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };
}

internal static class IzipayValidation
{
    public static DateTimeOffset NormalizeExpiration(DateTimeOffset requested)
    {
        var utc = requested.ToUniversalTime();
        var minute = utc.Minute < 30 ? 0 : 30;
        var normalized = new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, minute, 0, TimeSpan.Zero);
        return normalized <= utc ? normalized.AddMinutes(30) : normalized;
    }

    public static string? ValidateContact(IzipayPaymentLinkContact contact)
    {
        if (contact is null || contact.FirstName.Trim().Length is < 2 or > 50 || contact.LastName.Trim().Length is < 2 or > 50)
        {
            return "El nombre y apellido para Izipay deben tener entre 2 y 50 caracteres.";
        }

        if (contact.Email.Trim().Length is < 5 or > 50 || !contact.Email.Contains('@', StringComparison.Ordinal))
        {
            return "El correo para Izipay no es valido.";
        }

        if (contact.PhoneNumber.Trim().Length is < 7 or > 15 || contact.Street.Trim().Length is < 5 or > 25)
        {
            return "El telefono debe tener entre 7 y 15 caracteres y la direccion entre 5 y 25.";
        }

        if (contact.PostalCode.Trim().Length is < 5 or > 10 || contact.City.Trim().Length is < 3 or > 40 || contact.State.Trim().Length is < 3 or > 40)
        {
            return "Completa un codigo postal, ciudad y departamento validos para Izipay.";
        }

        if (string.Equals(contact.DocumentType.Trim(), "DNI", StringComparison.OrdinalIgnoreCase) &&
            contact.Document.Trim().Length != 8)
        {
            return "El DNI debe tener 8 caracteres.";
        }

        return null;
    }
}

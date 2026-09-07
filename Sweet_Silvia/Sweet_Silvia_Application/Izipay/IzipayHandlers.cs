using System.Globalization;
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

        var expirationDate = request.ExpirationDate ?? DateTimeOffset.UtcNow.AddHours(24);
        if (expirationDate <= DateTimeOffset.UtcNow || expirationDate > DateTimeOffset.UtcNow.AddDays(7))
        {
            return Result.Failure<IzipayPaymentLinkResponse>(ApplicationErrors.Conflict(
                "Izipay.InvalidExpiration",
                "Izipay payment link expiration should be between now and seven days."));
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
                request.Shipping ?? request.Billing),
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
        if (!isCommunicationError &&
            (string.IsNullOrWhiteSpace(request.PayloadHttp) ||
             string.IsNullOrWhiteSpace(request.Signature) ||
             !signatureValidator.IsValid(request.PayloadHttp, request.Signature)))
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.InvalidSignature",
                "Izipay notification signature is invalid."));
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

        if (payment.GatewayExpiresAt.HasValue && payment.GatewayExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            payment.ExpireGateway();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.PaymentExpired",
                "The Izipay payment link has expired."));
        }

        if (isCommunicationError)
        {
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        var gatewayOrder = request.Response?.Order?.FirstOrDefault();
        if (string.Equals(request.Code, "00", StringComparison.OrdinalIgnoreCase))
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

            var settlement = await PaymentSettlementService.Approve(
                payment,
                null,
                gatewayOrder.CodeAuth,
                $"Izipay payment approved ({request.Code}).",
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
                return Result.Failure<IzipayNotificationResult>(settlement.Error);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }

        payment.RejectFromGateway($"Izipay payment rejected ({request.Code}): {request.Message?.Trim()}");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new IzipayNotificationResult(payment.Id, payment.Status);
    }
}

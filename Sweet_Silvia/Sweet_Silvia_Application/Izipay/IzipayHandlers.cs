using System.Globalization;
using System.Text.Json;
using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Coupons;
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
using CartEntity = Sweet_Silvia_Domain.Cart.Cart;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;

namespace Sweet_Silvia_Application.Izipay;

internal sealed class GenerateIzipayPaymentFormCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<PaymentMethodEntity> paymentMethods,
    IIzipayPaymentGateway izipayPaymentGateway,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GenerateIzipayPaymentFormCommand, IzipayPaymentFormResponse>
{
    public async Task<Result<IzipayPaymentFormResponse>> Handle(GenerateIzipayPaymentFormCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var paymentMethod = await paymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null || !paymentMethod.IsActive)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.NotFound("PaymentMethod", request.PaymentMethodId));
        }

        if (paymentMethod.Type != PaymentMethodType.Gateway || !paymentMethod.RequiresExternalIntegration)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.Conflict(
                "PaymentMethod.InvalidGateway",
                "The selected payment method is not configured as an external gateway."));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "A payment form can only be generated for an order awaiting payment."));
        }

        var customerError = IzipayCustomerValidation.Validate(request.Customer);
        if (customerError is not null)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.Conflict("Izipay.InvalidCustomer", customerError));
        }

        var now = DateTimeOffset.UtcNow;
        var pendingPayments = payments.Query()
            .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.PendingGateway)
            .ToList();

        var expiredPayments = pendingPayments
            .Where(payment => payment.GatewayExpiresAt.HasValue && payment.GatewayExpiresAt.Value <= now)
            .ToList();
        foreach (var expiredPayment in expiredPayments)
        {
            expiredPayment.ExpireGateway("Izipay payment session expired.");
        }
        if (expiredPayments.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var activePayments = payments.Query()
            .Where(payment =>
                payment.OrderId == order.Id &&
                payment.Status == PaymentStatus.PendingGateway &&
                (!payment.GatewayExpiresAt.HasValue || payment.GatewayExpiresAt.Value > now))
            .ToList();
        if (activePayments.Count > 1)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.Conflict(
                "Payment.MultipleActiveGatewayPayments",
                "Este pedido tiene varios intentos Izipay activos y requiere revision antes de continuar."));
        }

        var activePayment = activePayments.SingleOrDefault();
        if (activePayment is not null && !request.ReplaceActivePayment)
        {
            return Result.Failure<IzipayPaymentFormResponse>(ApplicationErrors.Conflict(
                "Payment.ActiveGatewayPaymentExists",
                "Este pedido ya tiene un pago Izipay activo. Espera la confirmacion o vuelve a intentarlo cuando figure como rechazado."));
        }

        var orderReference = activePayment?.GatewayTransactionId ?? Guid.NewGuid().ToString("N");
        var gatewayResult = await izipayPaymentGateway.CreatePaymentFormAsync(
            new IzipayPaymentFormGatewayRequest(
                orderReference,
                order.Total.Amount,
                order.Total.Currency,
                request.Customer),
            cancellationToken);

        if (gatewayResult.IsFailure)
        {
            return Result.Failure<IzipayPaymentFormResponse>(gatewayResult.Error);
        }

        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
        var payment = activePayment;
        if (payment is null)
        {
            payment = PaymentEntity.CreateGatewayPending(
                order.Id,
                paymentMethod.Id,
                order.Total,
                orderReference,
                orderReference,
                expiresAt,
                "FORM_TOKEN_CREATED");
            payments.Add(payment);
        }
        else
        {
            payment.RefreshGatewaySession(expiresAt, "FORM_TOKEN_REFRESHED");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var amountMinorUnits = decimal.ToInt64(order.Total.Amount * 100m);
        return new IzipayPaymentFormResponse(
            payment.Id,
            order.Id,
            orderReference,
            gatewayResult.Value.FormToken,
            gatewayResult.Value.PublicKey,
            amountMinorUnits,
            order.Total.Currency);
    }

}

internal sealed class ProcessIzipayNotificationCommandHandler(
    IIzipaySignatureValidator signatureValidator,
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IRepository<CartEntity> carts,
    IRepository<CartItemEntity> cartItems,
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IRepository<Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory> histories,
    IRepository<CouponEntity> coupons,
    IRepository<CouponUsageEntity> couponUsages,
    ICouponUsageLock couponUsageLock,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProcessIzipayNotificationCommand, IzipayNotificationResult>
{
    public async Task<Result<IzipayNotificationResult>> Handle(ProcessIzipayNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.HashAlgorithm, "sha256_hmac", StringComparison.OrdinalIgnoreCase) ||
            !(request.IsServerNotification
                ? signatureValidator.IsNotificationValid(request.Answer, request.Signature)
                : signatureValidator.IsClientConfirmationValid(request.Answer, request.Signature)))
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.InvalidSignature",
                "Izipay notification signature is invalid."));
        }

        JsonDocument notification;
        try
        {
            notification = JsonDocument.Parse(request.Answer);
        }
        catch (JsonException)
        {
            return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                "Izipay.InvalidPayload",
                "Izipay notification answer is not valid JSON."));
        }

        using (notification)
        {
            var root = notification.RootElement;
            var shopId = GetString(root, "shopId");
            var orderDetails = GetProperty(root, "orderDetails");
            var orderReference = GetString(orderDetails, "orderId");
            var orderStatus = GetString(root, "orderStatus");
            var orderCycle = GetString(root, "orderCycle");

            var shopMatches = string.Equals(shopId, request.ExpectedShopId, StringComparison.Ordinal);
            var isAuthenticatedClientConfirmation = request.ExpectedPaymentId.HasValue && string.IsNullOrWhiteSpace(shopId);
            if ((!shopMatches && !isAuthenticatedClientConfirmation) ||
                string.IsNullOrWhiteSpace(orderReference) ||
                string.IsNullOrWhiteSpace(orderStatus))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.InvalidPayload",
                    "Izipay notification does not match this shop or has no order reference."));
            }

            var payment = payments.Query().FirstOrDefault(item => item.GatewayTransactionId == orderReference);
            if (payment is null)
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.NotFound("Payment"));
            }

            if (request.ExpectedPaymentId.HasValue && payment.Id != request.ExpectedPaymentId.Value)
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.PaymentMismatch",
                    "Izipay response does not match the selected payment."));
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

            if (!string.Equals(orderStatus, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(orderCycle, "CLOSED", StringComparison.OrdinalIgnoreCase))
                {
                    payment.RejectFromGateway($"Izipay payment finished with status {orderStatus}.");
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return new IzipayNotificationResult(payment.Id, payment.Status);
            }

            var reportedAmount = GetDecimal(orderDetails, "orderTotalAmount");
            var reportedCurrency = GetString(orderDetails, "orderCurrency");
            if (reportedAmount is null || reportedAmount.Value != payment.Amount.Amount * 100m ||
                !string.Equals(reportedCurrency, payment.Amount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.Conflict(
                    "Izipay.AmountMismatch",
                    "Izipay notification amount or currency does not match the payment."));
            }

            var authorizationCode = GetAuthorizationCode(root);
            var settlementOrder = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
            if (settlementOrder is null)
            {
                return Result.Failure<IzipayNotificationResult>(ApplicationErrors.NotFound("Order", payment.OrderId));
            }

            await using var couponLock = settlementOrder.CouponId is Guid couponId
                ? await couponUsageLock.AcquireAsync(couponId, cancellationToken)
                : null;
            var settlement = await PaymentSettlementService.Approve(
                payment,
                null,
                authorizationCode,
                "Izipay payment approved (PAID).",
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
                    payment.MarkGatewaySettlementPending(
                        "Izipay aprobo el pago, pero el stock requiere conciliacion manual: " + settlement.Error.Message);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    return new IzipayNotificationResult(payment.Id, payment.Status);
                }

                return Result.Failure<IzipayNotificationResult>(settlement.Error);
            }

            var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
            var cart = order is null
                ? null
                : carts.Query().FirstOrDefault(item => item.UserId == order.UserId && item.IsActive);
            if (cart is not null)
            {
                var purchasedQuantities = orderItems.Query()
                    .Where(item => item.OrderId == payment.OrderId)
                    .GroupBy(item => item.ProductVariantId)
                    .Select(group => new
                    {
                        ProductVariantId = group.Key,
                        Quantity = group.Sum(item => item.Quantity)
                    })
                    .ToList();

                foreach (var purchased in purchasedQuantities)
                {
                    var cartItem = cartItems.Query().FirstOrDefault(item =>
                        item.CartId == cart.Id && item.ProductVariantId == purchased.ProductVariantId);
                    if (cartItem is null)
                    {
                        continue;
                    }

                    if (purchased.Quantity >= cartItem.Quantity)
                    {
                        cartItems.Remove(cartItem);
                    }
                    else
                    {
                        cartItem.DecreaseQuantity(purchased.Quantity);
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new IzipayNotificationResult(payment.Id, payment.Status);
        }
    }

    private static string? GetAuthorizationCode(JsonElement root)
    {
        var transactions = GetProperty(root, "transactions");
        if (transactions.ValueKind == JsonValueKind.Array && transactions.GetArrayLength() > 0)
        {
            var first = transactions[0];
            var authorization = GetProperty(first, "authorizationResponse");
            return GetString(authorization, "authorizationNumber") ?? GetString(first, "uuid");
        }

        return null;
    }

    private static decimal? GetDecimal(JsonElement element, string name)
    {
        var property = GetProperty(element, name);
        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
        {
            return number;
        }

        return property.ValueKind == JsonValueKind.String &&
               decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number)
            ? number
            : null;
    }

    private static string? GetString(JsonElement element, string name)
    {
        var property = GetProperty(element, name);
        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static JsonElement GetProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return default;
    }
}

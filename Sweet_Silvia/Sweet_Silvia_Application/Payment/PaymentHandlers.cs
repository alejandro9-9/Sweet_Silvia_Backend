using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Coupons;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.PaymentReceipt;
using Sweet_Silvia_Application.PaymentReview;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Coupon;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.PaymentMethod;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.StockMovement;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using OrderStatusHistoryEntity = Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using PaymentReceiptEntity = Sweet_Silvia_Domain.PaymentReceipt.PaymentReceipt;
using PaymentReviewEntity = Sweet_Silvia_Domain.PaymentReview.PaymentReview;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;

namespace Sweet_Silvia_Application.Payment;

internal sealed class CreatePendingPaymentCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IRepository<PaymentMethodEntity> paymentMethods,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePendingPaymentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePendingPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Order", request.OrderId));
        }

        var paymentMethod = await paymentMethods.GetByIdAsync(request.PaymentMethodId, cancellationToken);
        if (paymentMethod is null || !paymentMethod.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("PaymentMethod", request.PaymentMethodId));
        }

        if (paymentMethod.RequiresExternalIntegration)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "PaymentMethod.ExternalIntegrationRequired",
                "This payment method requires a gateway integration flow."));
        }

        if (paymentMethod.Type == PaymentMethodType.Cash && (order.ShippingAgencyId.HasValue || !order.AddressId.HasValue))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "PaymentMethod.CashUnavailableForOlva",
                "Cash payment is not available for Olva Courier shipments."));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "A payment can only be created for an order awaiting payment."));
        }

        var hasActivePayment = payments.Query().Any(payment =>
            payment.OrderId == order.Id &&
            (payment.Status == PaymentStatus.PendingReceipt ||
             payment.Status == PaymentStatus.InReview ||
             payment.Status == PaymentStatus.PendingGateway ||
             payment.Status == PaymentStatus.Approved));
        if (hasActivePayment)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Payment.ActivePaymentExists",
                "This order already has an active payment."));
        }

        var amount = Money.Create(request.Amount, request.Currency);
        if (order.Total.Amount != amount.Amount || order.Total.Currency != amount.Currency)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("Payment.InvalidAmount", "Payment amount should match the order total."));
        }

        var payment = PaymentEntity.CreatePending(
            request.OrderId,
            request.PaymentMethodId,
            amount);

        payments.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}

internal sealed class MarkPaymentReceiptUploadedCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MarkPaymentReceiptUploadedCommand>
{
    public async Task<Result> Handle(MarkPaymentReceiptUploadedCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Payment", request.PaymentId));
        }

        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", payment.OrderId));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "A receipt can only be uploaded for an order awaiting payment."));
        }

        payment.MarkReceiptUploaded();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetPaymentByIdQueryHandler(IRepository<PaymentEntity> payments)
    : IQueryHandler<GetPaymentByIdQuery, PaymentResponse>
{
    public Task<Result<PaymentResponse>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = payments.Query().FirstOrDefault(payment => payment.Id == request.Id);
        return Task.FromResult(payment is null
            ? Result.Failure<PaymentResponse>(ApplicationErrors.NotFound("Payment", request.Id))
            : Result.Success(PaymentMappings.ToResponse(payment)));
    }
}

internal sealed class GetPaymentsQueryHandler(IRepository<PaymentEntity> payments)
    : IQueryHandler<GetPaymentsQuery, List<PaymentResponse>>
{
    public Task<Result<List<PaymentResponse>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = payments.Query();
        if (request.Status.HasValue)
        {
            query = query.Where(payment => payment.Status == request.Status.Value);
        }

        return Task.FromResult(Result.Success(query.Select(PaymentMappings.ToResponse).ToList()));
    }
}

internal sealed class GetPaymentsByOrderQueryHandler(IRepository<PaymentEntity> payments)
    : IQueryHandler<GetPaymentsByOrderQuery, List<PaymentResponse>>
{
    public Task<Result<List<PaymentResponse>>> Handle(GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(payments.Query()
            .Where(payment => payment.OrderId == request.OrderId)
            .Select(PaymentMappings.ToResponse)
            .ToList()));
    }
}

internal sealed class UploadPaymentReceiptCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<PaymentReceiptEntity> receipts,
    IRepository<OrderEntity> orders,
    IRepository<OrderStatusHistoryEntity> histories,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UploadPaymentReceiptCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UploadPaymentReceiptCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Payment", request.PaymentId));
        }

        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Order", payment.OrderId));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "A receipt can only be uploaded for an order awaiting payment."));
        }

        var declaredAmount = request.DeclaredAmount.HasValue ? Money.Create(request.DeclaredAmount.Value, request.Currency) : null;
        if (declaredAmount is not null && (declaredAmount.Amount != payment.Amount.Amount || declaredAmount.Currency != payment.Amount.Currency))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("PaymentReceipt.InvalidAmount", "Declared receipt amount should match the payment amount."));
        }

        var receipt = PaymentReceiptEntity.Create(
            request.PaymentId,
            request.FileUrl,
            request.OperationCode,
            declaredAmount);

        payment.MarkReceiptUploaded();
        receipts.Add(receipt);

        if (order.Status == OrderStatus.PendingReceipt)
        {
            var previousStatus = order.Status;
            order.ChangeStatus(OrderStatus.ReceiptInReview);
            histories.Add(OrderStatusHistoryEntity.Create(order.Id, previousStatus, OrderStatus.ReceiptInReview, null, "Payment receipt uploaded."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return receipt.Id;
    }
}

internal sealed class GetPaymentReceiptsByPaymentQueryHandler(IRepository<PaymentReceiptEntity> receipts)
    : IQueryHandler<GetPaymentReceiptsByPaymentQuery, List<PaymentReceiptResponse>>
{
    public Task<Result<List<PaymentReceiptResponse>>> Handle(GetPaymentReceiptsByPaymentQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(receipts.Query()
            .Where(receipt => receipt.PaymentId == request.PaymentId)
            .Select(PaymentMappings.ToReceiptResponse)
            .ToList()));
    }
}

internal sealed class GetPaymentReceiptByIdQueryHandler(IRepository<PaymentReceiptEntity> receipts)
    : IQueryHandler<GetPaymentReceiptByIdQuery, PaymentReceiptResponse>
{
    public Task<Result<PaymentReceiptResponse>> Handle(GetPaymentReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var receipt = receipts.Query().FirstOrDefault(receipt => receipt.Id == request.Id);
        return Task.FromResult(receipt is null
            ? Result.Failure<PaymentReceiptResponse>(ApplicationErrors.NotFound("PaymentReceipt", request.Id))
            : Result.Success(PaymentMappings.ToReceiptResponse(receipt)));
    }
}

internal sealed class ReviewPaymentCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<PaymentReceiptEntity> receipts,
    IRepository<PaymentReviewEntity> reviews,
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IRepository<OrderStatusHistoryEntity> histories,
    IRepository<CouponEntity> coupons,
    IRepository<CouponUsageEntity> couponUsages,
    IRepository<PaymentMethodEntity> paymentMethods,
    ICouponUsageLock couponUsageLock,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReviewPaymentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ReviewPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Payment", request.PaymentId));
        }

        var paymentMethod = await paymentMethods.GetByIdAsync(payment.PaymentMethodId, cancellationToken);
        if (paymentMethod is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("PaymentMethod", payment.PaymentMethodId));
        }

        var isCashPayment = paymentMethod.Type == PaymentMethodType.Cash;
        if ((!isCashPayment && payment.Status != PaymentStatus.InReview) ||
            (isCashPayment && payment.Status is not PaymentStatus.PendingReceipt and not PaymentStatus.InReview))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Payment.NotInReview",
                "Only a payment in review or a pending cash payment can be reviewed manually."));
        }

        if (request.Result is not PaymentReviewResult.Approved and not PaymentReviewResult.Rejected)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "PaymentReview.InvalidResult",
                "Payment review result should be Approved or Rejected."));
        }

        var settlementOrder = request.Result == PaymentReviewResult.Approved
            ? await orders.GetByIdAsync(payment.OrderId, cancellationToken)
            : null;
        if (request.Result == PaymentReviewResult.Approved && settlementOrder is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.NotFound("Order", payment.OrderId));
        }

        await using var couponLock = settlementOrder?.CouponId is Guid couponId
            ? await couponUsageLock.AcquireAsync(couponId, cancellationToken)
            : null;

        var review = PaymentReviewEntity.Create(request.PaymentId, request.ReviewerUserId, request.Result, request.Observation);
        reviews.Add(review);
        var latestReceipt = receipts.Query()
            .Where(receipt => receipt.PaymentId == request.PaymentId)
            .OrderByDescending(receipt => receipt.UploadedAt)
            .FirstOrDefault();

        if (request.Result == PaymentReviewResult.Approved)
        {
            var stockValidation = await PaymentSettlementService.Approve(payment, request.ReviewerUserId, null, request.Observation, isCashPayment, orders, orderItems, variants, stockMovements, histories, coupons, couponUsages, cancellationToken);
            if (stockValidation.IsFailure)
            {
                return Result.Failure<Guid>(stockValidation.Error);
            }

            if (latestReceipt?.Status == ReceiptStatus.Uploaded)
            {
                latestReceipt.Approve(request.Observation);
            }
        }
        else
        {
            if (isCashPayment && payment.Status == PaymentStatus.PendingReceipt)
            {
                payment.RejectCash(request.ReviewerUserId, request.Observation);
            }
            else
            {
                payment.Reject(request.ReviewerUserId, request.Observation);
            }

            if (latestReceipt?.Status == ReceiptStatus.Uploaded)
            {
                latestReceipt.Reject(request.Observation);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return review.Id;
    }

}

internal static class PaymentSettlementService
{
    public static async Task<Result> Approve(
        PaymentEntity payment,
        Guid? reviewerUserId,
        string? gatewayAuthorizationCode,
        string? observation,
        bool isCashPayment,
        IRepository<OrderEntity> orders,
        IRepository<OrderItemEntity> orderItems,
        IRepository<ProductVariantEntity> variants,
        IRepository<StockMovementEntity> stockMovements,
        IRepository<OrderStatusHistoryEntity> histories,
        IRepository<CouponEntity> coupons,
        IRepository<CouponUsageEntity> couponUsages,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", payment.OrderId));
        }

        if (order.Status is not OrderStatus.PendingReceipt and not OrderStatus.ReceiptInReview)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Order.InvalidPaymentStatus",
                "Only orders awaiting payment can be settled."));
        }

        var items = orderItems.Query().Where(item => item.OrderId == order.Id).ToList();
        if (items.Count == 0)
        {
            return Result.Failure(ApplicationErrors.Conflict("Order.Empty", "The order has no items to settle."));
        }

        CouponUsageEntity? couponUsage = null;
        if (order.CouponId.HasValue && !couponUsages.Query().Any(item => item.OrderId == order.Id))
        {
            var coupon = await coupons.GetByIdAsync(order.CouponId.Value, cancellationToken);
            if (coupon is null)
            {
                return Result.Failure(ApplicationErrors.NotFound("Coupon", order.CouponId.Value));
            }

            if (coupon.Scope == CouponScope.Order)
            {
                couponUsage = CouponUsageEntity.Create(
                    coupon.Id,
                    order.UserId,
                    order.Id,
                    CouponScope.Order,
                    null,
                    null,
                    order.DiscountTotal);
            }
            else
            {
                var discountedItem = items.FirstOrDefault(item => item.HistoricalUnitDiscount.Amount > 0);
                if (discountedItem is null)
                {
                    return Result.Failure(ApplicationErrors.Conflict("Coupon.NotApplicable", "The order does not contain a discounted item."));
                }

                couponUsage = CouponUsageEntity.Create(
                    coupon.Id,
                    order.UserId,
                    order.Id,
                    CouponScope.Item,
                    discountedItem.Id,
                    Math.Min(order.CouponAppliedQuantity ?? 1, discountedItem.Quantity),
                    order.DiscountTotal);
            }

        }

        var reservationsByVariant = stockMovements.Query()
            .Where(movement => movement.OrderId == order.Id && movement.Type == StockMovementType.Reservation)
            .ToList()
            .GroupBy(movement => movement.ProductVariantId)
            .ToDictionary(group => group.Key, group => group.Sum(movement => movement.Quantity));
        var itemVariants = new List<(Guid ProductVariantId, int Quantity, ProductVariantEntity Variant, bool HasReservation)>();
        foreach (var itemGroup in items.GroupBy(item => item.ProductVariantId))
        {
            var variant = await variants.GetByIdAsync(itemGroup.Key, cancellationToken);
            if (variant is null || !variant.IsActive)
            {
                return Result.Failure(ApplicationErrors.NotFound("ProductVariant", itemGroup.Key));
            }

            var quantity = itemGroup.Sum(item => item.Quantity);
            var reservedQuantity = reservationsByVariant.GetValueOrDefault(itemGroup.Key);
            var hasReservation = reservedQuantity == quantity;

            if (reservedQuantity > 0 && !hasReservation)
            {
                return Result.Failure(ApplicationErrors.Conflict(
                    "Stock.ReservationMismatch",
                    $"The stock reservation for product variant '{variant.Id}' does not match the order quantity."));
            }

            if (!hasReservation && variant.PhysicalStock < quantity)
            {
                return Result.Failure(ApplicationErrors.Conflict("Stock.NotEnough", $"Product variant '{variant.Id}' does not have enough stock."));
            }

            itemVariants.Add((itemGroup.Key, quantity, variant, hasReservation));
        }

        foreach (var (productVariantId, quantity, variant, hasReservation) in itemVariants)
        {
            if (hasReservation)
            {
                continue;
            }

            var previousStock = variant.PhysicalStock;
            variant.DecreaseStock(quantity);
            stockMovements.Add(StockMovementEntity.Create(
                productVariantId,
                order.Id,
                StockMovementType.Sale,
                quantity,
                previousStock,
                variant.PhysicalStock,
                "Payment approved."));
        }

        if (reviewerUserId.HasValue)
        {
            if (isCashPayment && payment.Status == PaymentStatus.PendingReceipt)
            {
                payment.ApproveCash(reviewerUserId.Value, observation);
            }
            else
            {
                payment.Approve(reviewerUserId.Value, observation);
            }
        }
        else
        {
            payment.ApproveFromGateway(gatewayAuthorizationCode, observation);
        }

        var previousStatus = order.Status;
        order.ChangeStatus(OrderStatus.Paid, reviewerUserId, observation);
        histories.Add(OrderStatusHistoryEntity.Create(order.Id, previousStatus, OrderStatus.Paid, reviewerUserId, observation));

        if (couponUsage is not null)
        {
            couponUsages.Add(couponUsage);
        }

        return Result.Success();
    }
}

internal sealed class GetPaymentReviewsQueryHandler(IRepository<PaymentReviewEntity> reviews)
    : IQueryHandler<GetPaymentReviewsQuery, List<PaymentReviewResponse>>
{
    public Task<Result<List<PaymentReviewResponse>>> Handle(GetPaymentReviewsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(reviews.Query().Select(PaymentMappings.ToReviewResponse).ToList()));
    }
}

internal sealed class GetPaymentReviewsByPaymentQueryHandler(IRepository<PaymentReviewEntity> reviews)
    : IQueryHandler<GetPaymentReviewsByPaymentQuery, List<PaymentReviewResponse>>
{
    public Task<Result<List<PaymentReviewResponse>>> Handle(GetPaymentReviewsByPaymentQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(reviews.Query()
            .Where(review => review.PaymentId == request.PaymentId)
            .Select(PaymentMappings.ToReviewResponse)
            .ToList()));
    }
}

internal static class PaymentMappings
{
    public static PaymentResponse ToResponse(PaymentEntity payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.PaymentMethodId,
            payment.Status,
            payment.Amount.Amount,
            payment.Currency,
            payment.TransactionCode,
            payment.AuthorizationCode,
            payment.GatewayTransactionId,
            payment.GatewayExpiresAt,
            payment.CreatedAt,
            payment.PaidAt,
            payment.ReceiptUploadedAt,
            payment.ReviewedAt,
            payment.ReviewedByUserId,
            payment.Observation);
    }

    public static PaymentReceiptResponse ToReceiptResponse(PaymentReceiptEntity receipt)
    {
        return new PaymentReceiptResponse(
            receipt.Id,
            receipt.PaymentId,
            receipt.FileUrl,
            receipt.OperationCode,
            receipt.DeclaredAmount?.Amount,
            receipt.DeclaredAmount?.Currency,
            receipt.UploadedAt,
            receipt.Status,
            receipt.Observation);
    }

    public static PaymentReviewResponse ToReviewResponse(PaymentReviewEntity review)
    {
        return new PaymentReviewResponse(
            review.Id,
            review.PaymentId,
            review.ReviewerUserId,
            review.Result,
            review.ReviewedAt,
            review.Observation);
    }
}

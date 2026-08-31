using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.PaymentReceipt;
using Sweet_Silvia_Application.PaymentReview;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Order;
using Sweet_Silvia_Domain.Payment;
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

namespace Sweet_Silvia_Application.Payment;

internal sealed class CreatePendingPaymentCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<OrderEntity> orders,
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

internal sealed class MarkPaymentReceiptUploadedCommandHandler(IRepository<PaymentEntity> payments, IUnitOfWork unitOfWork)
    : ICommandHandler<MarkPaymentReceiptUploadedCommand>
{
    public async Task<Result> Handle(MarkPaymentReceiptUploadedCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Payment", request.PaymentId));
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

        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order is not null && order.Status == OrderStatus.PendingReceipt)
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

internal sealed class ReviewPaymentCommandHandler(
    IRepository<PaymentEntity> payments,
    IRepository<PaymentReviewEntity> reviews,
    IRepository<OrderEntity> orders,
    IRepository<OrderItemEntity> orderItems,
    IRepository<ProductVariantEntity> variants,
    IRepository<StockMovementEntity> stockMovements,
    IRepository<OrderStatusHistoryEntity> histories,
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

        var review = PaymentReviewEntity.Create(request.PaymentId, request.ReviewerUserId, request.Result, request.Observation);
        reviews.Add(review);

        if (request.Result == PaymentReviewResult.Approved)
        {
            var stockValidation = await ApprovePaymentAndDiscountStock(payment, request.ReviewerUserId, request.Observation, orders, orderItems, variants, stockMovements, histories, cancellationToken);
            if (stockValidation.IsFailure)
            {
                return Result.Failure<Guid>(stockValidation.Error);
            }
        }
        else
        {
            payment.Reject(request.ReviewerUserId, request.Observation);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return review.Id;
    }

    private static async Task<Result> ApprovePaymentAndDiscountStock(
        PaymentEntity payment,
        Guid reviewerUserId,
        string? observation,
        IRepository<OrderEntity> orders,
        IRepository<OrderItemEntity> orderItems,
        IRepository<ProductVariantEntity> variants,
        IRepository<StockMovementEntity> stockMovements,
        IRepository<OrderStatusHistoryEntity> histories,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Order", payment.OrderId));
        }

        foreach (var item in orderItems.Query().Where(item => item.OrderId == order.Id).ToList())
        {
            var variant = await variants.GetByIdAsync(item.ProductVariantId, cancellationToken);
            if (variant is null || !variant.IsActive)
            {
                return Result.Failure(ApplicationErrors.NotFound("ProductVariant", item.ProductVariantId));
            }

            if (variant.PhysicalStock < item.Quantity)
            {
                return Result.Failure(ApplicationErrors.Conflict("Stock.NotEnough", $"Product variant '{variant.Id}' does not have enough stock."));
            }

            var previousStock = variant.PhysicalStock;
            variant.DecreaseStock(item.Quantity);
            stockMovements.Add(StockMovementEntity.Create(
                variant.Id,
                order.Id,
                StockMovementType.Sale,
                item.Quantity,
                previousStock,
                variant.PhysicalStock,
                "Payment approved."));
        }

        payment.Approve(reviewerUserId, observation);

        var previousStatus = order.Status;
        order.ChangeStatus(OrderStatus.Paid, reviewerUserId, observation);
        histories.Add(OrderStatusHistoryEntity.Create(order.Id, previousStatus, OrderStatus.Paid, reviewerUserId, observation));

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

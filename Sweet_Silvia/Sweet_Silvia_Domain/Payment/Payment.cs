using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Payment;

public sealed class Payment : Entity
{
    private Payment() { }

    public Payment(Guid id, Guid orderId, Guid paymentMethodId, PaymentStatus status, Money amount, string currency, string? transactionCode, string? authorizationCode, DateTimeOffset createdAt)
        : base(id)
    {
        OrderId = orderId;
        PaymentMethodId = paymentMethodId;
        Status = status;
        Amount = amount;
        Currency = currency;
        TransactionCode = transactionCode;
        AuthorizationCode = authorizationCode;
        CreatedAt = createdAt;
    }

    public Guid OrderId { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Money Amount { get; private set; } = null!;
    public string Currency { get; private set; } = string.Empty;
    public string? TransactionCode { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ReceiptUploadedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? Observation { get; private set; }

    public static Payment CreatePending(Guid orderId, Guid paymentMethodId, Money amount)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (paymentMethodId == Guid.Empty)
        {
            throw new ArgumentException("Payment method id is required.", nameof(paymentMethodId));
        }

        if (amount is null)
        {
            throw new ArgumentNullException(nameof(amount));
        }

        if (amount.Amount <= 0)
        {
            throw new ArgumentException("Payment amount should be greater than zero.", nameof(amount));
        }

        var result = new Payment(Guid.NewGuid(), orderId, paymentMethodId, PaymentStatus.PendingReceipt, amount, amount.Currency, null, null, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new PaymentCreatedEvent(result.Id));
        return result;
    }

    public void MarkReceiptUploaded()
    {
        if (Status != PaymentStatus.PendingReceipt && Status != PaymentStatus.Rejected)
        {
            throw new InvalidOperationException("Receipt can only be uploaded when payment is pending or rejected.");
        }

        Status = PaymentStatus.InReview;
        ReceiptUploadedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PaymentReceiptUploadedEvent(Id));
    }

    public void Approve(Guid reviewedByUserId, string? observation = null)
    {
        if (Status != PaymentStatus.InReview)
        {
            throw new InvalidOperationException("Payment can only be approved when it is in review.");
        }

        if (reviewedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer user id is required.", nameof(reviewedByUserId));
        }

        Status = PaymentStatus.Approved;
        PaidAt = DateTimeOffset.UtcNow;
        ReviewedAt = DateTimeOffset.UtcNow;
        ReviewedByUserId = reviewedByUserId;
        Observation = observation;
        AddDomainEvent(new PaymentApprovedEvent(Id));
    }

    public void Reject(Guid reviewedByUserId, string? observation = null)
    {
        if (Status != PaymentStatus.InReview)
        {
            throw new InvalidOperationException("Payment can only be rejected when it is in review.");
        }

        if (reviewedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer user id is required.", nameof(reviewedByUserId));
        }

        Status = PaymentStatus.Rejected;
        ReviewedAt = DateTimeOffset.UtcNow;
        ReviewedByUserId = reviewedByUserId;
        Observation = observation;
        AddDomainEvent(new PaymentRejectedEvent(Id));
    }
}

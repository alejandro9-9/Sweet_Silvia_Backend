using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Payment;

public sealed class Payment : Entity
{
    private Payment() { }

    public Payment(Guid id, Guid orderId, Guid paymentMethodId, PaymentStatus status, Money amount, string currency, string? transactionCode, string? authorizationCode, string? gatewayTransactionId, DateTimeOffset createdAt)
        : base(id)
    {
        OrderId = orderId;
        PaymentMethodId = paymentMethodId;
        Status = status;
        Amount = amount;
        Currency = currency;
        TransactionCode = transactionCode;
        AuthorizationCode = authorizationCode;
        GatewayTransactionId = gatewayTransactionId;
        CreatedAt = createdAt;
    }

    public Guid OrderId { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Money Amount { get; private set; } = null!;
    public string Currency { get; private set; } = string.Empty;
    public string? TransactionCode { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public DateTimeOffset? GatewayExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ReceiptUploadedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public string? Observation { get; private set; }

    public static Payment CreatePending(Guid orderId, Guid paymentMethodId, Money amount)
    {
        ValidateCreation(orderId, paymentMethodId, amount);

        var result = new Payment(Guid.NewGuid(), orderId, paymentMethodId, PaymentStatus.PendingReceipt, amount, amount.Currency, null, null, null, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new PaymentCreatedEvent(result.Id));
        return result;
    }

    public static Payment CreateGatewayPending(Guid orderId, Guid paymentMethodId, Money amount, string gatewayTransactionId, string gatewayReference, DateTimeOffset gatewayExpiresAt, string? gatewayState)
    {
        ValidateCreation(orderId, paymentMethodId, amount);

        if (string.IsNullOrWhiteSpace(gatewayTransactionId))
        {
            throw new ArgumentException("Gateway transaction id is required.", nameof(gatewayTransactionId));
        }

        if (string.IsNullOrWhiteSpace(gatewayReference))
        {
            throw new ArgumentException("Gateway reference is required.", nameof(gatewayReference));
        }

        if (gatewayExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Gateway expiration date should be in the future.", nameof(gatewayExpiresAt));
        }

        var result = new Payment(
            Guid.NewGuid(),
            orderId,
            paymentMethodId,
            PaymentStatus.PendingGateway,
            amount,
            amount.Currency,
            gatewayReference.Trim(),
            string.IsNullOrWhiteSpace(gatewayState) ? null : gatewayState.Trim(),
            gatewayTransactionId.Trim(),
            DateTimeOffset.UtcNow);

        result.GatewayExpiresAt = gatewayExpiresAt;

        result.AddDomainEvent(new PaymentCreatedEvent(result.Id));
        return result;
    }

    public void MarkReceiptUploaded()
    {
        if (!string.IsNullOrWhiteSpace(GatewayTransactionId))
        {
            throw new InvalidOperationException("A gateway payment cannot be settled with a manual receipt.");
        }

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

    public void ApproveCash(Guid reviewedByUserId, string? observation = null)
    {
        if (Status != PaymentStatus.PendingReceipt)
        {
            throw new InvalidOperationException("Cash payment can only be approved when it is pending.");
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

    public void RejectCash(Guid reviewedByUserId, string? observation = null)
    {
        if (Status != PaymentStatus.PendingReceipt)
        {
            throw new InvalidOperationException("Cash payment can only be rejected when it is pending.");
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

    public void ApproveFromGateway(string? authorizationCode = null, string? observation = null)
    {
        if (Status != PaymentStatus.PendingGateway)
        {
            throw new InvalidOperationException("Gateway payment can only be approved when it is pending.");
        }

        Status = PaymentStatus.Approved;
        PaidAt = DateTimeOffset.UtcNow;
        ReviewedAt = DateTimeOffset.UtcNow;
        AuthorizationCode = string.IsNullOrWhiteSpace(authorizationCode) ? AuthorizationCode : authorizationCode.Trim();
        Observation = observation?.Trim();
        AddDomainEvent(new PaymentApprovedEvent(Id));
    }

    public void RejectFromGateway(string? observation = null)
    {
        if (Status != PaymentStatus.PendingGateway)
        {
            throw new InvalidOperationException("Gateway payment can only be rejected when it is pending.");
        }

        Status = PaymentStatus.Rejected;
        ReviewedAt = DateTimeOffset.UtcNow;
        Observation = observation?.Trim();
        AddDomainEvent(new PaymentRejectedEvent(Id));
    }

    public void MarkGatewaySettlementPending(string observation)
    {
        if (Status != PaymentStatus.PendingGateway)
        {
            throw new InvalidOperationException("Only a pending gateway payment can require settlement review.");
        }

        Observation = observation.Trim();
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public void ExpireGateway(string? observation = null)
    {
        if (Status != PaymentStatus.PendingGateway)
        {
            return;
        }

        Status = PaymentStatus.Rejected;
        ReviewedAt = DateTimeOffset.UtcNow;
        Observation = observation?.Trim() ?? "Payment link expired.";
        AddDomainEvent(new PaymentRejectedEvent(Id));
    }

    public void RefreshGatewaySession(DateTimeOffset expiresAt, string? gatewayState)
    {
        if (Status != PaymentStatus.PendingGateway || expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Only an active gateway payment can refresh its session.");
        }

        GatewayExpiresAt = expiresAt;
        AuthorizationCode = string.IsNullOrWhiteSpace(gatewayState) ? AuthorizationCode : gatewayState.Trim();
    }

    private static void ValidateCreation(Guid orderId, Guid paymentMethodId, Money amount)
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
    }
}

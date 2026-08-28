using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Domain.PaymentReview;

public sealed class PaymentReview : Entity
{
    private PaymentReview() { }

    public PaymentReview(Guid id, Guid paymentId, Guid reviewerUserId, PaymentReviewResult result, DateTimeOffset reviewedAt, string? observation)
        : base(id)
    {
        PaymentId = paymentId;
        ReviewerUserId = reviewerUserId;
        Result = result;
        ReviewedAt = reviewedAt;
        Observation = observation;
    }

    public Guid PaymentId { get; private set; }
    public Guid ReviewerUserId { get; private set; }
    public PaymentReviewResult Result { get; private set; }
    public DateTimeOffset ReviewedAt { get; private set; }
    public string? Observation { get; private set; }

    public static PaymentReview Create(Guid paymentId, Guid reviewerUserId, PaymentReviewResult result, string? observation = null)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("Payment id is required.", nameof(paymentId));
        }

        if (reviewerUserId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer user id is required.", nameof(reviewerUserId));
        }

        return new PaymentReview(Guid.NewGuid(), paymentId, reviewerUserId, result, DateTimeOffset.UtcNow, observation?.Trim());
    }
}

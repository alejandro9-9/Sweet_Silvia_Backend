using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Application.PaymentReview;

public sealed record PaymentReviewResponse(Guid Id, Guid PaymentId, Guid ReviewerUserId, PaymentReviewResult Result, DateTimeOffset ReviewedAt, string? Observation);

public sealed record ReviewPaymentCommand(Guid PaymentId, Guid ReviewerUserId, PaymentReviewResult Result, string? Observation) : ICommand<Guid>;

public sealed record GetPaymentReviewsQuery() : IQuery<List<PaymentReviewResponse>>;

public sealed record GetPaymentReviewsByPaymentQuery(Guid PaymentId) : IQuery<List<PaymentReviewResponse>>;

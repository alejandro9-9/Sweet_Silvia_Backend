using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Application.Payment;

public sealed record PaymentResponse(Guid Id, Guid OrderId, Guid PaymentMethodId, PaymentStatus Status, decimal Amount, string Currency, string? TransactionCode, string? AuthorizationCode, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt, DateTimeOffset? ReceiptUploadedAt, DateTimeOffset? ReviewedAt, Guid? ReviewedByUserId, string? Observation);

public sealed record CreatePendingPaymentCommand(Guid OrderId, Guid PaymentMethodId, decimal Amount, string Currency = "PEN") : ICommand<Guid>;

public sealed record MarkPaymentReceiptUploadedCommand(Guid PaymentId) : ICommand;

public sealed record ApprovePaymentCommand(Guid PaymentId, Guid ReviewedByUserId, string? Observation) : ICommand;

public sealed record RejectPaymentCommand(Guid PaymentId, Guid ReviewedByUserId, string? Observation) : ICommand;

public sealed record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentResponse>;

public sealed record GetPaymentsByOrderQuery(Guid OrderId) : IQuery<List<PaymentResponse>>;

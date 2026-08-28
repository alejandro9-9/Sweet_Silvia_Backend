using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Payment;

namespace Sweet_Silvia_Application.PaymentReceipt;

public sealed record PaymentReceiptResponse(Guid Id, Guid PaymentId, string FileUrl, string? OperationCode, decimal? DeclaredAmount, string? Currency, DateTimeOffset UploadedAt, ReceiptStatus Status, string? Observation);

public sealed record UploadPaymentReceiptCommand(Guid PaymentId, string FileUrl, string? OperationCode, decimal? DeclaredAmount, string Currency = "PEN") : ICommand<Guid>;

public sealed record GetPaymentReceiptsByPaymentQuery(Guid PaymentId) : IQuery<List<PaymentReceiptResponse>>;

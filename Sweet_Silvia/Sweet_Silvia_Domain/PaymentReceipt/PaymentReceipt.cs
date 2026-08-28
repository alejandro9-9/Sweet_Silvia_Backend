using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Payment;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.PaymentReceipt;

public sealed class PaymentReceipt : Entity
{
    private PaymentReceipt() { }

    public PaymentReceipt(Guid id, Guid paymentId, string fileUrl, string? operationCode, Money? declaredAmount, DateTimeOffset uploadedAt, ReceiptStatus status, string? observation)
        : base(id)
    {
        PaymentId = paymentId;
        FileUrl = fileUrl;
        OperationCode = operationCode;
        DeclaredAmount = declaredAmount;
        UploadedAt = uploadedAt;
        Status = status;
        Observation = observation;
    }

    public Guid PaymentId { get; private set; }
    public string FileUrl { get; private set; } = string.Empty;
    public string? OperationCode { get; private set; }
    public Money? DeclaredAmount { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public ReceiptStatus Status { get; private set; }
    public string? Observation { get; private set; }

    public static PaymentReceipt Create(Guid paymentId, string fileUrl, string? operationCode, Money? declaredAmount)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("Payment id is required.", nameof(paymentId));
        }

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("Receipt file url is required.", nameof(fileUrl));
        }

        return new PaymentReceipt(Guid.NewGuid(), paymentId, fileUrl.Trim(), operationCode?.Trim(), declaredAmount, DateTimeOffset.UtcNow, ReceiptStatus.Uploaded, null);
    }
}

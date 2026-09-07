namespace Sweet_Silvia_Domain.Payment;

public enum PaymentStatus
{
    PendingReceipt = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    Voided = 5,
    PendingGateway = 6
}

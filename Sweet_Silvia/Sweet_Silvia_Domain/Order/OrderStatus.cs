namespace Sweet_Silvia_Domain.Order;

public enum OrderStatus
{
    PendingReceipt = 1,
    ReceiptInReview = 2,
    Paid = 3,
    Preparing = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
    OutOfStock = 8
}

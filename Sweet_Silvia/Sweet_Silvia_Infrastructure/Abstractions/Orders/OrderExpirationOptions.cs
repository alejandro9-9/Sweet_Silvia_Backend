namespace Sweet_Silvia_Infrastructure.Abstractions.Orders;

public sealed class OrderExpirationOptions
{
    public int PendingPaymentExpirationHours { get; set; } = 24;
}

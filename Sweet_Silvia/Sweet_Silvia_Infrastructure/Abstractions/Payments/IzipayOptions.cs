namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

public sealed class IzipayOptions
{
    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = "https://api.micuentaweb.pe/api-payment";
    public string PaymentEndpoint { get; init; } = "V4/Charge/CreatePayment";
    public string ShopId { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string HmacSha256Key { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
    public string NotificationUrl { get; init; } = string.Empty;
    public string Environment { get; init; } = "Sandbox";
}

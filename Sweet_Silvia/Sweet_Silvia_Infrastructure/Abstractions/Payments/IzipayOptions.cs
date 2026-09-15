namespace Sweet_Silvia_Infrastructure.Abstractions.Payments;

public sealed class IzipayOptions
{
    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = "https://sandbox-api-pw.izipay.pe";
    public string TokenEndpoint { get; init; } = "security/v1/Token/Generate";
    public string PaymentLinkEndpoint { get; init; } = "paymentlink/api/v1/process/generate";
    public string MerchantCode { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    // Backward-compatible alias for environments configured before ApiKey was introduced.
    public string PublicKey { get; init; } = string.Empty;
    public string HashKey { get; init; } = string.Empty;
    public string NotificationUrl { get; init; } = string.Empty;
    public string TermsAndConditionsUrl { get; init; } = string.Empty;
    public string Environment { get; init; } = "Sandbox";
}

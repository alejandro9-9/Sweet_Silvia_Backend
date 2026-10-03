namespace Sweet_Silvia_Infrastructure.Abstractions.Shipping;

public sealed class OlvaOptions
{
    public string BaseUrl { get; init; } = "https://api.olva-api.lat";
    public string ApiKey { get; init; } = string.Empty;
    public bool UseLocalQuoteEstimateWhenUnconfigured { get; init; }
}

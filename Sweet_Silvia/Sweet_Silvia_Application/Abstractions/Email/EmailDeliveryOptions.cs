namespace Sweet_Silvia_Application.Abstractions.Email;

public sealed class EmailDeliveryOptions
{
    public string Provider { get; init; } = "Brevo";

    public string ApiKey { get; init; } = string.Empty;

    public string From { get; init; } = string.Empty;
}

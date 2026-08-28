namespace Sweet_Silvia_Application.Abstractions.Email;

public sealed class EmailVerificationOptions
{
    public int CodeExpirationMinutes { get; init; } = 15;

    public int ResendCooldownSeconds { get; init; } = 60;
}

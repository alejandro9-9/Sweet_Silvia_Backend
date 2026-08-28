namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

public sealed class RefreshTokenOptions
{
    public int ExpirationDays { get; init; } = 30;
}

namespace Sweet_Silvia_Application.Abstractions.Security;

public interface IRefreshTokenService
{
    RefreshTokenValue Generate(DateTimeOffset createdAt);

    string Hash(string token);
}

public sealed record RefreshTokenValue(
    string Token,
    string TokenHash,
    DateTimeOffset ExpiresAt);

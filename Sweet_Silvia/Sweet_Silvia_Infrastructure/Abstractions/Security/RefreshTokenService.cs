using System.Security.Cryptography;
using System.Text;
using Sweet_Silvia_Application.Abstractions.Security;

namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

internal sealed class RefreshTokenService(RefreshTokenOptions options) : IRefreshTokenService
{
    public RefreshTokenValue Generate(DateTimeOffset createdAt)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new RefreshTokenValue(
            token,
            Hash(token),
            createdAt.AddDays(options.ExpirationDays));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}

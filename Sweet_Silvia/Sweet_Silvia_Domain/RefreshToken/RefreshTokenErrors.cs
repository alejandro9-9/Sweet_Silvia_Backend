using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.RefreshToken;

public static class RefreshTokenErrors
{
    public static Error NotFound => new("RefreshToken.NotFound", "The refresh token was not found.");

    public static Error Expired => new("RefreshToken.Expired", "The refresh token has expired.");

    public static Error Revoked => new("RefreshToken.Revoked", "The refresh token was revoked.");
}

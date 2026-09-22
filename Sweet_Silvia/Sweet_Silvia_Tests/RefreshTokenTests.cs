using Sweet_Silvia_Domain.RefreshToken;

namespace Sweet_Silvia_Tests;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Revoked_token_is_not_valid()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            "hash-1",
            createdAt.AddDays(30),
            "Chrome",
            "127.0.0.1",
            "test-agent",
            createdAt);

        token.Revoke(createdAt.AddMinutes(1));

        Assert.True(token.IsRevoked);
        Assert.False(token.IsValid(createdAt.AddMinutes(2)));
    }

    [Fact]
    public void Rotating_token_replaces_hash_and_keeps_it_valid()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = RefreshToken.Create(
            Guid.NewGuid(),
            "hash-1",
            createdAt.AddDays(30),
            null,
            null,
            null,
            createdAt);

        token.MarkUsed(createdAt.AddMinutes(1), null, null);
        token.Rotate("hash-2", createdAt.AddDays(30));

        Assert.Equal("hash-2", token.TokenHash);
        Assert.False(token.IsRevoked);
        Assert.True(token.IsValid(createdAt.AddMinutes(2)));
    }
}

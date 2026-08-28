using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.RefreshToken;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? deviceInfo,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? deviceInfo,
        DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("Expiration date should be greater than creation date.", nameof(expiresAt));
        }

        var token = new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash.Trim(),
            expiresAt,
            NormalizeDeviceInfo(deviceInfo),
            createdAt);

        token.AddDomainEvent(new RefreshTokenCreatedEvent(token.Id));
        return token;
    }

    public bool IsExpired(DateTimeOffset currentDate) => currentDate >= ExpiresAt;

    public bool IsValid(DateTimeOffset currentDate) => !IsRevoked && !IsExpired(currentDate);

    public void Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
        {
            return;
        }

        if (revokedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(revokedAt));
        }

        RevokedAt = revokedAt;
        AddDomainEvent(new RefreshTokenRevokedEvent(Id));
    }

    private static string? NormalizeDeviceInfo(string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
        {
            return null;
        }

        var normalized = deviceInfo.Trim();
        return normalized[..Math.Min(normalized.Length, 500)];
    }
}

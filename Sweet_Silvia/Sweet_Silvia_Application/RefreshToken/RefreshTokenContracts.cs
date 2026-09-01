using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.RefreshToken;

public sealed record RefreshTokenResponse(
    Guid Id,
    Guid UserId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset CreatedAt);

public sealed record LoginCommand(string Email, string Password, string? DeviceInfo, string? IpAddress, string? UserAgent) : ICommand<LoginResponse>;

public sealed record RefreshSessionCommand(string RefreshToken, string? DeviceInfo, string? IpAddress, string? UserAgent) : ICommand<LoginResponse>;

public sealed record RevokeSessionCommand(string RefreshToken, string? IpAddress, string? UserAgent) : ICommand;

public sealed record LoginResponse(string Token, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

public sealed record GetRefreshTokensByUserQuery(Guid UserId) : IQuery<List<RefreshTokenResponse>>;

public sealed record RevokeRefreshTokenCommand(Guid Id) : ICommand;

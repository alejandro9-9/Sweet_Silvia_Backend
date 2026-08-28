using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.RefreshToken;

public sealed record RefreshTokenResponse(Guid Id, Guid UserId, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt, string? DeviceInfo, DateTimeOffset CreatedAt);

public sealed record LoginCommand(string Email, string Password, string? DeviceInfo) : ICommand<LoginResponse>;

public sealed record RefreshSessionCommand(string RefreshToken, string? DeviceInfo) : ICommand<LoginResponse>;

public sealed record RevokeSessionCommand(string RefreshToken) : ICommand;

public sealed record LoginResponse(string Token, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

public sealed record GetRefreshTokensByUserQuery(Guid UserId) : IQuery<List<RefreshTokenResponse>>;

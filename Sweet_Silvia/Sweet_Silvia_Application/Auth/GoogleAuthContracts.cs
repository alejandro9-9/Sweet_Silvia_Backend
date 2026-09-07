using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.RefreshToken;

namespace Sweet_Silvia_Application.Auth;

public sealed record GoogleLoginCommand(
    string? IdToken,
    string? AuthorizationCode,
    Guid CustomerRoleId,
    string? Name,
    string? PaternalSurname,
    string? MaternalSurname,
    string? Phone,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent) : ICommand<LoginResponse>;

using Sweet_Silvia_Application.Abstractions.Audit;
using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Security;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.RefreshToken;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.RefreshToken;
using Sweet_Silvia_Domain.Role;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using UserEntity = Sweet_Silvia_Domain.User.User;
using RoleEntity = Sweet_Silvia_Domain.Role.Role;
using RefreshTokenEntity = Sweet_Silvia_Domain.RefreshToken.RefreshToken;

namespace Sweet_Silvia_Application.Auth;

internal sealed class GoogleLoginCommandHandler(
    IRepository<UserEntity> users,
    IRepository<RoleEntity> roles,
    IGoogleIdTokenValidator googleIdTokenValidator,
    IRefreshTokenService refreshTokenService,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokens,
    IAuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GoogleLoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (!googleIdTokenValidator.IsConfigured)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Conflict(
                "GoogleAuth.NotConfigured",
                "Google authentication is not configured."));
        }

        var profile = await googleIdTokenValidator.ValidateAsync(request.IdToken, request.AuthorizationCode, cancellationToken);
        if (profile is null || !profile.EmailVerified)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Unauthorized);
        }

        var email = EmailAddress.Create(profile.Email);
        var user = users.Query().FirstOrDefault(entry => entry.GoogleSubject == profile.Subject);

        if (user is null)
        {
            user = users.Query().FirstOrDefault(entry => entry.Email == email);

            if (user is not null)
            {
                if (user.GoogleSubject is not null && !string.Equals(user.GoogleSubject, profile.Subject, StringComparison.Ordinal))
                {
                    return Result.Failure<LoginResponse>(ApplicationErrors.Unauthorized);
                }

                user.LinkGoogleAccount(profile.Subject);
            }
            else
            {
                var customerRole = await roles.GetByIdAsync(request.CustomerRoleId, cancellationToken);
                if (customerRole is null || !customerRole.IsActive)
                {
                    return Result.Failure<LoginResponse>(ApplicationErrors.Conflict(
                        "Role.Invalid",
                        "The customer role does not exist or is inactive."));
                }

                var phone = string.IsNullOrWhiteSpace(request.Phone)
                    ? null
                    : PhoneNumber.Create(request.Phone);

                user = UserEntity.CreateFromGoogle(
                    customerRole.Id,
                    ResolveName(request.Name, profile),
                    ResolvePaternalSurname(request.PaternalSurname, profile),
                    request.MaternalSurname,
                    phone,
                    email,
                    profile.Subject);

                users.Add(user);
            }
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Unauthorized);
        }

        var role = roles.Query().FirstOrDefault(entry => entry.Id == user.RoleId);
        if (role is null || !role.IsActive)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Conflict("Role.Invalid", "The user role is invalid."));
        }

        var now = DateTimeOffset.UtcNow;
        var refreshToken = refreshTokenService.Generate(now);

        refreshTokens.Add(RefreshTokenEntity.Create(
            user.Id,
            refreshToken.TokenHash,
            refreshToken.ExpiresAt,
            request.DeviceInfo,
            request.IpAddress,
            request.UserAgent,
            now));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogWriter.WriteAsync(
            user.Id,
            role.Name,
            "User.GoogleLogin",
            "User",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            null,
            new { user.Email.Value, request.DeviceInfo },
            null,
            cancellationToken);

        var accessToken = jwtTokenGenerator.GenerateToken(Sweet_Silvia_Application.User.LoginCommandHandler.CreateClaims(user, role));
        return new LoginResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }

    private static string ResolveName(string? requestedName, GoogleAccountInfo profile)
    {
        if (!string.IsNullOrWhiteSpace(requestedName))
        {
            return requestedName;
        }

        if (!string.IsNullOrWhiteSpace(profile.GivenName))
        {
            return profile.GivenName;
        }

        var fullName = profile.Name?.Trim();
        return string.IsNullOrWhiteSpace(fullName) ? "Cliente" : fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
    }

    private static string ResolvePaternalSurname(string? requestedSurname, GoogleAccountInfo profile)
    {
        if (!string.IsNullOrWhiteSpace(requestedSurname))
        {
            return requestedSurname;
        }

        if (!string.IsNullOrWhiteSpace(profile.FamilyName))
        {
            return profile.FamilyName;
        }

        var nameParts = profile.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return nameParts.Length > 1 ? string.Join(' ', nameParts.Skip(1)) : "Google";
    }
}

using System.Security.Claims;
using Sweet_Silvia_Application.Abstractions.Audit;
using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Application.Abstractions.Security;
using Sweet_Silvia_Application.Common;
using Sweet_Silvia_Application.RefreshToken;
using Sweet_Silvia_Application.Role;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.RefreshToken;
using Sweet_Silvia_Domain.Role;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using Sweet_Silvia_Domain.User;
using UserEntity = Sweet_Silvia_Domain.User.User;
using RoleEntity = Sweet_Silvia_Domain.Role.Role;
using RefreshTokenEntity = Sweet_Silvia_Domain.RefreshToken.RefreshToken;

namespace Sweet_Silvia_Application.User;

internal sealed class CreateUserCommandHandler(
    IRepository<UserEntity> users,
    IRepository<RoleEntity> roles,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Password.Length is < 8 or > 128)
        {
            return Result.Failure<Guid>(ApplicationErrors.Validation(
                "User.PasswordInvalid",
                "Password must contain between 8 and 128 characters."));
        }

        var role = await roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict(
                "Role.Invalid",
                "The selected role does not exist or is inactive."));
        }

        var email = EmailAddress.Create(request.Email);
        var phone = PhoneNumber.Create(request.Phone);

        if (users.Query().Any(user => user.Email == email))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("User.EmailAlreadyExists", "Email is already registered."));
        }

        var user = UserEntity.Create(
            request.RoleId,
            request.Name,
            request.PaternalSurname,
            request.MaternalSurname,
            phone,
            email,
            passwordHasher.Hash(request.Password));

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}

internal sealed class ChangeUserRoleCommandHandler(
    IRepository<UserEntity> users,
    IRepository<RoleEntity> roles,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<Result> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("User", request.UserId));
        }

        var role = await roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "Role.Invalid",
                "The selected role does not exist or is inactive."));
        }

        if (user.RoleId == RoleIds.Administrator && request.RoleId != RoleIds.Administrator &&
            users.Query().Count(item => item.RoleId == RoleIds.Administrator && item.Status == UserStatus.Active) <= 1)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "User.LastAdministrator",
                "The last active administrator cannot be demoted."));
        }

        user.ChangeRole(request.RoleId);
        var now = DateTimeOffset.UtcNow;
        var activeTokens = await refreshTokens.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class UpdateUserProfileCommandHandler(
    IRepository<UserEntity> users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserProfileCommand, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserResponse>(ApplicationErrors.NotFound("User", request.UserId));
        }

        var email = EmailAddress.Create(request.Email);
        var phone = PhoneNumber.Create(request.Phone);
        if (users.Query().Any(entry => entry.Id != request.UserId && entry.Email == email))
        {
            return Result.Failure<UserResponse>(ApplicationErrors.Conflict("User.EmailAlreadyExists", "Email is already registered."));
        }

        user.UpdateProfile(request.Name, request.PaternalSurname, request.MaternalSurname, phone, email);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UserMappings.ToResponse(user);
    }
}

internal sealed class BlockUserCommandHandler(
    IRepository<UserEntity> users,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork)
    : ICommandHandler<BlockUserCommand>
{
    public async Task<Result> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("User", request.UserId));
        }

        if (user.RoleId == RoleIds.Administrator &&
            users.Query().Count(item => item.RoleId == RoleIds.Administrator && item.Status == UserStatus.Active) <= 1)
        {
            return Result.Failure(ApplicationErrors.Conflict(
                "User.LastAdministrator",
                "The last active administrator cannot be blocked."));
        }

        user.Block();
        var now = DateTimeOffset.UtcNow;
        var activeTokens = await refreshTokens.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class UnblockUserCommandHandler(
    IRepository<UserEntity> users,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UnblockUserCommand>
{
    public async Task<Result> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("User", request.UserId));
        }

        user.Unblock();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetUserByIdQueryHandler(IRepository<UserEntity> users)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = users.Query().FirstOrDefault(user => user.Id == request.Id);
        return Task.FromResult(user is null
            ? Result.Failure<UserResponse>(ApplicationErrors.NotFound("User", request.Id))
            : Result.Success(UserMappings.ToResponse(user)));
    }
}

internal sealed class GetUsersQueryHandler(IRepository<UserEntity> users)
    : IQueryHandler<GetUsersQuery, List<UserResponse>>
{
    public Task<Result<List<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = users.Query();

        if (request.RoleId.HasValue)
        {
            query = query.Where(user => user.RoleId == request.RoleId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(user => user.Status == request.Status);
        }

        return Task.FromResult(Result.Success(query.Select(UserMappings.ToResponse).ToList()));
    }
}

internal sealed class LoginCommandHandler(
    IRepository<UserEntity> users,
    IRepository<RoleEntity> roles,
    IRefreshTokenService refreshTokenService,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokens,
    IAuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(request.Email);
        var user = users.Query().FirstOrDefault(user => user.Email == email);

        if (user is null || user.PasswordHash is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.InvalidCredentials);
        }

        if (user.Status == UserStatus.Blocked)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.UserBlocked);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.UserInactive);
        }

        var role = roles.Query().FirstOrDefault(role => role.Id == user.RoleId);
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
            "User.Login",
            "User",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            null,
            new { user.Email.Value, request.DeviceInfo },
            null,
            cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(CreateClaims(user, role));
        return new LoginResponse(token, refreshToken.Token, refreshToken.ExpiresAt);
    }

    internal static IEnumerable<Claim> CreateClaims(UserEntity user, RoleEntity role)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, role.Name),
            new Claim("roleId", user.RoleId.ToString())
        ];
    }
}

internal sealed class RefreshSessionCommandHandler(
    IRepository<UserEntity> users,
    IRepository<RoleEntity> roles,
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenService refreshTokenService,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshSessionCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.Hash(request.RefreshToken);
        var token = await refreshTokens.GetByHashAsync(tokenHash, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (token is null || !token.IsValid(now))
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Unauthorized);
        }

        var user = await users.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Unauthorized);
        }

        var role = roles.Query().FirstOrDefault(role => role.Id == user.RoleId);
        if (role is null || !role.IsActive)
        {
            return Result.Failure<LoginResponse>(ApplicationErrors.Conflict("Role.Invalid", "The user role is invalid."));
        }

        token.MarkUsed(now, request.IpAddress, request.UserAgent);
        var nextRefreshToken = refreshTokenService.Generate(now);
        token.Rotate(nextRefreshToken.TokenHash, nextRefreshToken.ExpiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var accessToken = jwtTokenGenerator.GenerateToken(LoginCommandHandler.CreateClaims(user, role));
        return new LoginResponse(accessToken, nextRefreshToken.Token, nextRefreshToken.ExpiresAt);
    }
}

internal sealed class RevokeSessionCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenService refreshTokenService,
    IAuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokens.GetByHashAsync(refreshTokenService.Hash(request.RefreshToken), cancellationToken);
        if (token is not null)
        {
            token.Revoke(DateTimeOffset.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await auditLogWriter.WriteAsync(
                token.UserId,
                null,
                "User.Logout",
                "RefreshToken",
                token.Id,
                request.IpAddress,
                request.UserAgent,
                null,
                null,
                null,
                cancellationToken);
        }

        return Result.Success();
    }
}

internal sealed class GetRefreshTokensByUserQueryHandler(IRefreshTokenRepository refreshTokens)
    : IQueryHandler<GetRefreshTokensByUserQuery, List<RefreshTokenResponse>>
{
    public async Task<Result<List<RefreshTokenResponse>>> Handle(GetRefreshTokensByUserQuery request, CancellationToken cancellationToken)
    {
        var tokens = await refreshTokens.GetByUserIdAsync(request.UserId, cancellationToken);
        return tokens.Select(token => new RefreshTokenResponse(
            token.Id,
            token.UserId,
            token.ExpiresAt,
            token.RevokedAt,
            token.DeviceInfo,
            token.IpAddress,
            token.UserAgent,
            token.LastUsedAt,
            token.CreatedAt)).ToList();
    }
}

internal sealed class RevokeRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokens.GetByIdAsync(request.Id, cancellationToken);
        if (token is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("RefreshToken", request.Id));
        }

        token.Revoke(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class GetRolesQueryHandler(IRepository<RoleEntity> roles)
    : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    public Task<Result<List<RoleResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = request.OnlyActive ? roles.Query().Where(role => role.IsActive) : roles.Query();
        return Task.FromResult(Result.Success(query.Select(role => new RoleResponse(role.Id, role.Name, role.IsActive)).ToList()));
    }
}

internal sealed class CreateRoleCommandHandler(IRepository<RoleEntity> roles, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (roles.Query().Any(role => role.Name == request.Name))
        {
            return Result.Failure<Guid>(ApplicationErrors.Conflict("Role.NameAlreadyExists", "Role name already exists."));
        }

        var role = RoleEntity.Create(request.Id, request.Name);
        roles.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return role.Id;
    }
}

internal sealed class UpdateRoleCommandHandler(IRepository<RoleEntity> roles, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Role", request.Id));
        }

        role.Update(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteRoleCommandHandler(IRepository<RoleEntity> roles, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            return Result.Failure(ApplicationErrors.NotFound("Role", request.Id));
        }

        role.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class UserMappings
{
    public static UserResponse ToResponse(UserEntity user)
    {
        return new UserResponse(
            user.Id,
            user.RoleId,
            user.Name,
            user.PaternalSurname,
            user.MaternalSurname,
            user.Phone?.Value,
            user.Email.Value,
            user.Status,
            user.CreatedAt);
    }
}

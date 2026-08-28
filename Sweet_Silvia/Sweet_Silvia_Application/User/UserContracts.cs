using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.User;

namespace Sweet_Silvia_Application.User;

public sealed record UserResponse(Guid Id, Guid RoleId, string Name, string PaternalSurname, string? MaternalSurname, string Phone, string Email, UserStatus Status, DateTimeOffset CreatedAt);

public sealed record CreateUserCommand(Guid RoleId, string Name, string PaternalSurname, string? MaternalSurname, string Phone, string Email, string Password) : ICommand<Guid>;

public sealed record ChangeUserRoleCommand(Guid UserId, Guid RoleId) : ICommand;

public sealed record BlockUserCommand(Guid UserId) : ICommand;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserResponse>;

public sealed record GetUsersQuery(Guid? RoleId = null, UserStatus? Status = null) : IQuery<List<UserResponse>>;

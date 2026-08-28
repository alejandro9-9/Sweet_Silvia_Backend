using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Role;

public sealed record RoleResponse(Guid Id, string Name, bool IsActive);

public sealed record CreateRoleCommand(Guid Id, string Name) : ICommand<Guid>;

public sealed record GetRolesQuery(bool OnlyActive = true) : IQuery<List<RoleResponse>>;

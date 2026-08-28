using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Shared.Enums;

namespace Sweet_Silvia_Application.Department;

public sealed record DepartmentResponse(Guid Id, string Name, ShippingDestinationType DestinationType, bool IsActive);

public sealed record CreateDepartmentCommand(string Name, ShippingDestinationType DestinationType) : ICommand<Guid>;

public sealed record UpdateDepartmentCommand(Guid Id, string Name, ShippingDestinationType DestinationType) : ICommand;

public sealed record DeleteDepartmentCommand(Guid Id) : ICommand;

public sealed record GetDepartmentByIdQuery(Guid Id) : IQuery<DepartmentResponse>;

public sealed record GetDepartmentsQuery(bool OnlyActive = true) : IQuery<List<DepartmentResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Province;

public sealed record ProvinceResponse(Guid Id, Guid DepartmentId, string Name, bool IsActive);

public sealed record CreateProvinceCommand(Guid DepartmentId, string Name) : ICommand<Guid>;

public sealed record UpdateProvinceCommand(Guid Id, Guid DepartmentId, string Name) : ICommand;

public sealed record DeleteProvinceCommand(Guid Id) : ICommand;

public sealed record GetProvinceByIdQuery(Guid Id) : IQuery<ProvinceResponse>;

public sealed record GetProvincesByDepartmentQuery(Guid DepartmentId, bool OnlyActive = true) : IQuery<List<ProvinceResponse>>;

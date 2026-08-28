using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.District;

public sealed record DistrictResponse(Guid Id, Guid ProvinceId, string Name, string? Ubigeo, bool IsActive);

public sealed record CreateDistrictCommand(Guid ProvinceId, string Name, string? Ubigeo) : ICommand<Guid>;

public sealed record UpdateDistrictCommand(Guid Id, Guid ProvinceId, string Name, string? Ubigeo) : ICommand;

public sealed record DeleteDistrictCommand(Guid Id) : ICommand;

public sealed record GetDistrictByIdQuery(Guid Id) : IQuery<DistrictResponse>;

public sealed record GetDistrictsByProvinceQuery(Guid ProvinceId, bool OnlyActive = true) : IQuery<List<DistrictResponse>>;

using Sweet_Silvia_Application.Abstractions.Messaging;

namespace Sweet_Silvia_Application.Address;

public sealed record AddressResponse(Guid Id, Guid UserId, string ReceiverName, string ReceiverPhone, string Country, Guid DepartmentId, Guid ProvinceId, Guid DistrictId, string Line, string? References, bool IsDefault, bool IsActive);

public sealed record CreateAddressCommand(Guid UserId, string ReceiverName, string ReceiverPhone, Guid DepartmentId, Guid ProvinceId, Guid DistrictId, string Line, string? References, bool IsDefault) : ICommand<Guid>;

public sealed record UpdateAddressCommand(Guid Id, string ReceiverName, string ReceiverPhone, Guid DepartmentId, Guid ProvinceId, Guid DistrictId, string Line, string? References, bool IsDefault) : ICommand;

public sealed record DeleteAddressCommand(Guid Id) : ICommand;

public sealed record GetAddressByIdQuery(Guid Id) : IQuery<AddressResponse>;

public sealed record GetAddressesByUserQuery(Guid UserId, bool OnlyActive = true) : IQuery<List<AddressResponse>>;

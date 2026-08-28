using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.Courier;

namespace Sweet_Silvia_Application.Courier;

public sealed record CourierResponse(Guid Id, string Name, CourierType Type, bool RequiresExternalIntegration, bool IsActive);

public sealed record CreateCourierCommand(string Name, CourierType Type, bool RequiresExternalIntegration) : ICommand<Guid>;

public sealed record UpdateCourierCommand(Guid Id, string Name, CourierType Type, bool RequiresExternalIntegration) : ICommand;

public sealed record DeleteCourierCommand(Guid Id) : ICommand;

public sealed record GetCourierByIdQuery(Guid Id) : IQuery<CourierResponse>;

public sealed record GetCouriersQuery(bool OnlyActive = true) : IQuery<List<CourierResponse>>;

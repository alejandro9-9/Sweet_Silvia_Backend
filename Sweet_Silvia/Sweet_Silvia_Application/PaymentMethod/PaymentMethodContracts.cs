using Sweet_Silvia_Application.Abstractions.Messaging;
using Sweet_Silvia_Domain.PaymentMethod;

namespace Sweet_Silvia_Application.PaymentMethod;

public sealed record PaymentMethodResponse(Guid Id, string Name, PaymentMethodType Type, bool RequiresManualVerification, bool RequiresExternalIntegration, bool IsActive);

public sealed record CreatePaymentMethodCommand(string Name, PaymentMethodType Type, bool RequiresManualVerification, bool RequiresExternalIntegration) : ICommand<Guid>;

public sealed record UpdatePaymentMethodCommand(Guid Id, string Name, PaymentMethodType Type, bool RequiresManualVerification, bool RequiresExternalIntegration) : ICommand;

public sealed record DeletePaymentMethodCommand(Guid Id) : ICommand;

public sealed record ActivatePaymentMethodCommand(Guid Id) : ICommand;

public sealed record GetPaymentMethodByIdQuery(Guid Id) : IQuery<PaymentMethodResponse>;

public sealed record GetPaymentMethodsQuery(bool OnlyActive = true) : IQuery<List<PaymentMethodResponse>>;

using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.PaymentMethod;

public sealed class PaymentMethod : Entity
{
    private PaymentMethod() { }

    public PaymentMethod(Guid id, string name, PaymentMethodType type, bool requiresManualVerification, bool requiresExternalIntegration, bool isActive)
        : base(id)
    {
        Name = name;
        Type = type;
        RequiresManualVerification = requiresManualVerification;
        RequiresExternalIntegration = requiresExternalIntegration;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public PaymentMethodType Type { get; private set; }
    public bool RequiresManualVerification { get; private set; }
    public bool RequiresExternalIntegration { get; private set; }
    public bool IsActive { get; private set; }

    public static PaymentMethod Create(string name, PaymentMethodType type, bool requiresManualVerification, bool requiresExternalIntegration)
    {
        Validate(name, type, requiresManualVerification, requiresExternalIntegration);

        var result = new PaymentMethod(Guid.NewGuid(), name.Trim(), type, requiresManualVerification, requiresExternalIntegration, true);
        result.AddDomainEvent(new PaymentMethodCreatedEvent(result.Id));
        return result;
    }

    public void Update(string name, PaymentMethodType type, bool requiresManualVerification, bool requiresExternalIntegration)
    {
        Validate(name, type, requiresManualVerification, requiresExternalIntegration);

        Name = name.Trim();
        Type = type;
        RequiresManualVerification = requiresManualVerification;
        RequiresExternalIntegration = requiresExternalIntegration;
        AddDomainEvent(new PaymentMethodUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        AddDomainEvent(new PaymentMethodDeletedEvent(Id));
    }

    private static void Validate(string name, PaymentMethodType type, bool requiresManualVerification, bool requiresExternalIntegration)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Payment method name is required.", nameof(name));
        }

        if (type == PaymentMethodType.Gateway && (requiresManualVerification || !requiresExternalIntegration))
        {
            throw new ArgumentException("Gateway payment methods should require external integration and should not require manual verification.");
        }

        if (type == PaymentMethodType.Manual && (!requiresManualVerification || requiresExternalIntegration))
        {
            throw new ArgumentException("Manual payment methods should require manual verification and should not require external integration.");
        }

        if (type == PaymentMethodType.Cash && (requiresManualVerification || requiresExternalIntegration))
        {
            throw new ArgumentException("Cash payment methods should not require manual verification or external integration.");
        }
    }
}

using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Courier;

public sealed class Courier : Entity
{
    private Courier() { }

    public Courier(Guid id, string name, CourierType type, bool requiresExternalIntegration, bool isActive)
        : base(id)
    {
        Name = name;
        Type = type;
        RequiresExternalIntegration = requiresExternalIntegration;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public CourierType Type { get; private set; }
    public bool RequiresExternalIntegration { get; private set; }
    public bool IsActive { get; private set; }

    public static Courier Create(string name, CourierType type, bool requiresExternalIntegration)
    {
        Validate(name);

        var result = new Courier(Guid.NewGuid(), name.Trim(), type, requiresExternalIntegration, true);
        result.AddDomainEvent(new CourierCreatedEvent(result.Id));
        return result;
    }

    public void Update(string name, CourierType type, bool requiresExternalIntegration)
    {
        Validate(name);

        Name = name.Trim();
        Type = type;
        RequiresExternalIntegration = requiresExternalIntegration;
        AddDomainEvent(new CourierUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        AddDomainEvent(new CourierDeletedEvent(Id));
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Courier name is required.", nameof(name));
        }
    }
}

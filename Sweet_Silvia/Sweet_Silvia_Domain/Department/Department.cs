using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.Enums;

namespace Sweet_Silvia_Domain.Department;

public sealed class Department : Entity
{
    private Department() { }

    public Department(Guid id, string name, ShippingDestinationType destinationType, bool isActive)
        : base(id)
    {
        Name = name;
        DestinationType = destinationType;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public ShippingDestinationType DestinationType { get; private set; }
    public bool IsActive { get; private set; }

    public static Department Create(string name, ShippingDestinationType destinationType)
    {
        Validate(name);

        return new Department(Guid.NewGuid(), name.Trim(), destinationType, true);
    }

    public void Update(string name, ShippingDestinationType destinationType)
    {
        Validate(name);

        Name = name.Trim();
        DestinationType = destinationType;
    }

    public void Delete()
    {
        IsActive = false;
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.", nameof(name));
        }
    }
}

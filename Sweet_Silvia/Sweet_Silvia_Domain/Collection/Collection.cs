using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Collection;

public sealed class Collection : Entity
{
    private Collection() { }

    public Collection(Guid id, string name, string description, bool isActive)
        : base(id)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static Collection Create(string name, string description)
    {
        Validate(name, description);

        var result = new Collection(Guid.NewGuid(), name.Trim(), description.Trim(), true);
        result.AddDomainEvent(new CollectionCreatedEvent(result.Id));
        return result;
    }

    public void Update(string name, string description)
    {
        Validate(name, description);

        Name = name.Trim();
        Description = description.Trim();
        AddDomainEvent(new CollectionUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        AddDomainEvent(new CollectionDeletedEvent(Id));
    }

    private static void Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Collection description is required.", nameof(description));
        }
    }
}

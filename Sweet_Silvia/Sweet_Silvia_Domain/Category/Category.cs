using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.Category;

public sealed class Category : Entity
{
    private Category() { }

    public Category(Guid id, string name, string description, bool isActive)
        : base(id)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static Category Create(string name, string description)
    {
        Validate(name, description);

        var result = new Category(Guid.NewGuid(), name.Trim(), description.Trim(), true);
        result.AddDomainEvent(new CategoryCreatedEvent(result.Id));
        return result;
    }

    public void Update(string name, string description)
    {
        Validate(name, description);

        Name = name.Trim();
        Description = description.Trim();
        AddDomainEvent(new CategoryUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        AddDomainEvent(new CategoryDeletedEvent(Id));
    }

    private static void Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Category description is required.", nameof(description));
        }
    }
}

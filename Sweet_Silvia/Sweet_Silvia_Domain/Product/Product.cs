using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Product;

public sealed class Product : Entity
{
    private Product() { }

    public Product(Guid id, Guid categoryId, Guid? collectionId, string name, string? description, Money basePrice, bool isActive, DateTimeOffset createdAt)
        : base(id)
    {
        CategoryId = categoryId;
        CollectionId = collectionId;
        Name = name;
        Description = description;
        BasePrice = basePrice;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid CategoryId { get; private set; }
    public Guid? CollectionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Product Create(Guid categoryId, Guid? collectionId, string name, string? description, Money basePrice)
    {
        Validate(categoryId, name, basePrice);

        var result = new Product(Guid.NewGuid(), categoryId, collectionId, name.Trim(), description?.Trim(), basePrice, true, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new ProductCreatedEvent(result.Id));
        return result;
    }

    public void Update(string name, string? description, Money basePrice, Guid categoryId, Guid? collectionId)
    {
        Validate(categoryId, name, basePrice);

        Name = name.Trim();
        Description = description?.Trim();
        BasePrice = basePrice;
        CategoryId = categoryId;
        CollectionId = collectionId;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProductDeletedEvent(Id));
    }

    private static void Validate(Guid categoryId, string name, Money basePrice)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category id is required.", nameof(categoryId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (basePrice is null)
        {
            throw new ArgumentNullException(nameof(basePrice));
        }
    }
}

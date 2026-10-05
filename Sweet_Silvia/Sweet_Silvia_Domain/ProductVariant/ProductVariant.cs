using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.ProductVariant;

public sealed class ProductVariant : Entity
{
    private ProductVariant() { }

    public ProductVariant(Guid id, Guid productId, string size, string color, Sku sku, int physicalStock, Money? price, decimal shippingWeightKg, decimal shippingLengthCm, decimal shippingWidthCm, decimal shippingHeightCm, bool isActive, DateTimeOffset createdAt)
        : base(id)
    {
        ProductId = productId;
        Size = size;
        Color = color;
        Sku = sku;
        PhysicalStock = physicalStock;
        Price = price;
        ShippingWeightKg = shippingWeightKg;
        ShippingLengthCm = shippingLengthCm;
        ShippingWidthCm = shippingWidthCm;
        ShippingHeightCm = shippingHeightCm;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid ProductId { get; private set; }
    public string Size { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = null!;
    public int PhysicalStock { get; private set; }
    public Money? Price { get; private set; }
    public decimal ShippingWeightKg { get; private set; }
    public decimal ShippingLengthCm { get; private set; }
    public decimal ShippingWidthCm { get; private set; }
    public decimal ShippingHeightCm { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static ProductVariant Create(Guid productId, string size, string color, Sku sku, int physicalStock, Money? price = null, decimal shippingWeightKg = 0.3m, decimal shippingLengthCm = 30m, decimal shippingWidthCm = 22m, decimal shippingHeightCm = 4m)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(size))
        {
            throw new ArgumentException("Size is required.", nameof(size));
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Color is required.", nameof(color));
        }

        if (physicalStock < 0)
        {
            throw new ArgumentException("Physical stock cannot be negative.", nameof(physicalStock));
        }

        ValidateShippingDimensions(shippingWeightKg, shippingLengthCm, shippingWidthCm, shippingHeightCm);
        var result = new ProductVariant(Guid.NewGuid(), productId, size.Trim(), color.Trim(), sku, physicalStock, price, shippingWeightKg, shippingLengthCm, shippingWidthCm, shippingHeightCm, true, DateTimeOffset.UtcNow);
        result.AddDomainEvent(new ProductVariantCreatedEvent(result.Id));
        return result;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        if (PhysicalStock < quantity)
        {
            throw new InvalidOperationException("Not enough stock.");
        }

        PhysicalStock -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        PhysicalStock += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string size, string color, Money? price, decimal? shippingWeightKg = null, decimal? shippingLengthCm = null, decimal? shippingWidthCm = null, decimal? shippingHeightCm = null)
    {
        if (string.IsNullOrWhiteSpace(size))
        {
            throw new ArgumentException("Size is required.", nameof(size));
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Color is required.", nameof(color));
        }

        Size = size.Trim();
        Color = color.Trim();
        Price = price;
        var nextWeight = shippingWeightKg ?? ShippingWeightKg;
        var nextLength = shippingLengthCm ?? ShippingLengthCm;
        var nextWidth = shippingWidthCm ?? ShippingWidthCm;
        var nextHeight = shippingHeightCm ?? ShippingHeightCm;
        ValidateShippingDimensions(nextWeight, nextLength, nextWidth, nextHeight);
        ShippingWeightKg = nextWeight;
        ShippingLengthCm = nextLength;
        ShippingWidthCm = nextWidth;
        ShippingHeightCm = nextHeight;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProductVariantUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProductVariantDeletedEvent(Id));
    }

    private static void ValidateShippingDimensions(decimal weightKg, decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        if (weightKg <= 0 || lengthCm <= 0 || widthCm <= 0 || heightCm <= 0)
        {
            throw new ArgumentException("Shipping dimensions and weight must be greater than zero.");
        }
    }
}

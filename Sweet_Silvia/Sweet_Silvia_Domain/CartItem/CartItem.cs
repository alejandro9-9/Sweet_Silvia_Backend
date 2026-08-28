using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.CartItem;

public sealed class CartItem : Entity
{
    private CartItem() { }

    public CartItem(Guid id, Guid cartId, Guid productVariantId, int quantity, DateTimeOffset addedAt)
        : base(id)
    {
        CartId = cartId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        AddedAt = addedAt;
    }

    public Guid CartId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static CartItem Create(Guid cartId, Guid productVariantId, int quantity)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException("Cart id is required.", nameof(cartId));
        }

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        return new CartItem(Guid.NewGuid(), cartId, productVariantId, quantity, DateTimeOffset.UtcNow);
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        Quantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DecreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        if (quantity >= Quantity)
        {
            throw new ArgumentException("Quantity to decrease should be less than current quantity.", nameof(quantity));
        }

        Quantity -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

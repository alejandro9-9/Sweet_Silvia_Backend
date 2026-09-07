using Sweet_Silvia_Domain.Abstractions;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;

namespace Sweet_Silvia_Domain.Cart;

public sealed class Cart : Entity
{
    private readonly List<CartItemEntity> _items = [];

    private Cart() { }

    public Cart(Guid id, Guid userId, DateTimeOffset createdAt, DateTimeOffset updatedAt, bool isActive)
        : base(id)
    {
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsActive = isActive;
    }

    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<CartItemEntity> Items => _items.AsReadOnly();

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var now = DateTimeOffset.UtcNow;
        return new Cart(Guid.NewGuid(), userId, now, now, true);
    }

    public void AddItem(Guid productVariantId, int quantity)
    {
        EnsureActive();

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        var existingItem = _items.FirstOrDefault(item => item.ProductVariantId == productVariantId);

        if (existingItem is null)
        {
            _items.Add(CartItemEntity.Create(Id, productVariantId, quantity));
        }
        else
        {
            existingItem.IncreaseQuantity(quantity);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DecreaseItemQuantity(Guid productVariantId, int quantity)
    {
        EnsureActive();

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        var existingItem = FindItem(productVariantId);

        if (quantity >= existingItem.Quantity)
        {
            _items.Remove(existingItem);
        }
        else
        {
            existingItem.DecreaseQuantity(quantity);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateItemQuantity(Guid productVariantId, int quantity)
    {
        EnsureActive();

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (quantity <= 0)
        {
            RemoveItem(productVariantId);
            return;
        }

        var existingItem = FindItem(productVariantId);
        existingItem.SetQuantity(quantity);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveItem(Guid productVariantId)
    {
        EnsureActive();

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        var existingItem = _items.FirstOrDefault(item => item.ProductVariantId == productVariantId);

        if (existingItem is null)
        {
            return;
        }

        _items.Remove(existingItem);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Clear()
    {
        EnsureActive();

        if (_items.Count == 0)
        {
            return;
        }

        _items.Clear();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Touch()
    {
        EnsureActive();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private CartItemEntity FindItem(Guid productVariantId)
    {
        return _items.FirstOrDefault(item => item.ProductVariantId == productVariantId)
            ?? throw new InvalidOperationException("Cart item was not found.");
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cart is not active.");
        }
    }
}

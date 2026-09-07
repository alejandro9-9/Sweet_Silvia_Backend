using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;

namespace Sweet_Silvia_Domain.Order;

public sealed class Order : Entity
{
    private readonly List<OrderItemEntity> _items = [];

    private Order() { }

    public Order(Guid id, Guid userId, Guid addressId, Guid? couponId, int? couponAppliedQuantity, OrderStatus status, DateTimeOffset createdAt, Money subtotal, Money discountTotal, Money shippingCost, Money total)
        : base(id)
    {
        UserId = userId;
        AddressId = addressId;
        CouponId = couponId;
        CouponAppliedQuantity = couponAppliedQuantity;
        Status = status;
        CreatedAt = createdAt;
        Subtotal = subtotal;
        DiscountTotal = discountTotal;
        ShippingCost = shippingCost;
        Total = total;
    }

    public Guid UserId { get; private set; }
    public Guid AddressId { get; private set; }
    public Guid? CouponId { get; private set; }
    public int? CouponAppliedQuantity { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Money Subtotal { get; private set; } = null!;
    public Money DiscountTotal { get; private set; } = null!;
    public Money ShippingCost { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public IReadOnlyCollection<OrderItemEntity> Items => _items.AsReadOnly();

    public static Order Create(Guid userId, Guid addressId, Guid? couponId, int? couponAppliedQuantity, Money subtotal, Money discountTotal, Money shippingCost, Money total)
    {
        Validate(userId, addressId, subtotal, discountTotal, shippingCost, total);

        if (couponAppliedQuantity <= 0)
        {
            throw new ArgumentException("Applied coupon quantity should be greater than zero.", nameof(couponAppliedQuantity));
        }

        var result = new Order(Guid.NewGuid(), userId, addressId, couponId, couponAppliedQuantity, OrderStatus.PendingReceipt, DateTimeOffset.UtcNow, subtotal, discountTotal, shippingCost, total);
        result.AddDomainEvent(new OrderCreatedEvent(result.Id));
        return result;
    }

    public void AddItem(Guid productVariantId, string productName, string sku, string size, string color, int quantity, Money unitPrice, Money unitDiscount, Money subtotal)
    {
        _items.Add(OrderItemEntity.Create(Id, productVariantId, productName, sku, size, color, quantity, unitPrice, unitDiscount, subtotal));
    }

    public void ChangeStatus(OrderStatus status, Guid? changedByUserId = null, string? observation = null)
    {
        if (changedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Changed by user id is invalid.", nameof(changedByUserId));
        }

        if (!IsValidTransition(Status, status))
        {
            throw new InvalidOperationException($"Order cannot change from {Status} to {status}.");
        }

        var previousStatus = Status;
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new OrderStatusChangedEvent(Id, previousStatus, status, changedByUserId, observation));
    }

    private static bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            OrderStatus.PendingReceipt => next is OrderStatus.ReceiptInReview or OrderStatus.Cancelled or OrderStatus.OutOfStock,
            OrderStatus.ReceiptInReview => next is OrderStatus.PendingReceipt or OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.OutOfStock,
            OrderStatus.Paid => next is OrderStatus.Preparing,
            OrderStatus.Preparing => next is OrderStatus.Shipped,
            OrderStatus.Shipped => next is OrderStatus.Delivered,
            _ => false
        };
    }

    private static void Validate(Guid userId, Guid addressId, Money subtotal, Money discountTotal, Money shippingCost, Money total)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (addressId == Guid.Empty)
        {
            throw new ArgumentException("Address id is required.", nameof(addressId));
        }

        if (subtotal is null)
        {
            throw new ArgumentNullException(nameof(subtotal));
        }

        if (discountTotal is null)
        {
            throw new ArgumentNullException(nameof(discountTotal));
        }

        if (shippingCost is null)
        {
            throw new ArgumentNullException(nameof(shippingCost));
        }

        if (total is null)
        {
            throw new ArgumentNullException(nameof(total));
        }

        if (subtotal.Currency != discountTotal.Currency || subtotal.Currency != shippingCost.Currency || subtotal.Currency != total.Currency)
        {
            throw new ArgumentException("Order money values should use the same currency.");
        }

        if (discountTotal.Amount > subtotal.Amount)
        {
            throw new ArgumentException("Discount total cannot be greater than subtotal.", nameof(discountTotal));
        }

        var expectedTotal = subtotal.Amount - discountTotal.Amount + shippingCost.Amount;
        if (total.Amount != expectedTotal)
        {
            throw new ArgumentException("Order total should be subtotal minus discount plus shipping cost.", nameof(total));
        }
    }
}

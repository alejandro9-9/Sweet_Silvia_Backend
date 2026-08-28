using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.OrderItem;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    public OrderItem(Guid id, Guid orderId, Guid productVariantId, string historicalProductName, string historicalSku, string historicalSize, string historicalColor, int quantity, Money historicalUnitPrice, Money historicalUnitDiscount, Money subtotal)
        : base(id)
    {
        OrderId = orderId;
        ProductVariantId = productVariantId;
        HistoricalProductName = historicalProductName;
        HistoricalSku = historicalSku;
        HistoricalSize = historicalSize;
        HistoricalColor = historicalColor;
        Quantity = quantity;
        HistoricalUnitPrice = historicalUnitPrice;
        HistoricalUnitDiscount = historicalUnitDiscount;
        Subtotal = subtotal;
    }

    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string HistoricalProductName { get; private set; } = string.Empty;
    public string HistoricalSku { get; private set; } = string.Empty;
    public string HistoricalSize { get; private set; } = string.Empty;
    public string HistoricalColor { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money HistoricalUnitPrice { get; private set; } = null!;
    public Money HistoricalUnitDiscount { get; private set; } = null!;
    public Money Subtotal { get; private set; } = null!;

    public static OrderItem Create(Guid orderId, Guid productVariantId, string productName, string sku, string size, string color, int quantity, Money unitPrice, Money unitDiscount, Money subtotal)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is required.", nameof(orderId));
        }

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name is required.", nameof(productName));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required.", nameof(sku));
        }

        if (string.IsNullOrWhiteSpace(size))
        {
            throw new ArgumentException("Size is required.", nameof(size));
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Color is required.", nameof(color));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        return new OrderItem(Guid.NewGuid(), orderId, productVariantId, productName.Trim(), sku.Trim(), size.Trim(), color.Trim(), quantity, unitPrice, unitDiscount, subtotal);
    }
}

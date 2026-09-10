using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.StockMovement;

public sealed class StockMovement : Entity
{
    private StockMovement() { }

    public StockMovement(Guid id, Guid productVariantId, Guid? orderId, StockMovementType type, int quantity, int previousStock, int newStock, DateTimeOffset createdAt, string? observation)
        : base(id)
    {
        ProductVariantId = productVariantId;
        OrderId = orderId;
        Type = type;
        Quantity = quantity;
        PreviousStock = previousStock;
        NewStock = newStock;
        CreatedAt = createdAt;
        Observation = observation;
    }

    public Guid ProductVariantId { get; private set; }
    public Guid? OrderId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public int PreviousStock { get; private set; }
    public int NewStock { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string? Observation { get; private set; }

    public static StockMovement Create(Guid productVariantId, Guid? orderId, StockMovementType type, int quantity, int previousStock, int newStock, string? observation = null)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity should be greater than zero.", nameof(quantity));
        }

        if (previousStock < 0)
        {
            throw new ArgumentException("Previous stock cannot be negative.", nameof(previousStock));
        }

        if (newStock < 0)
        {
            throw new ArgumentException("New stock cannot be negative.", nameof(newStock));
        }

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order id is invalid.", nameof(orderId));
        }

        ValidateStockChange(type, quantity, previousStock, newStock, orderId);

        return new StockMovement(Guid.NewGuid(), productVariantId, orderId, type, quantity, previousStock, newStock, DateTimeOffset.UtcNow, observation?.Trim());
    }

    private static void ValidateStockChange(StockMovementType type, int quantity, int previousStock, int newStock, Guid? orderId)
    {
        if ((type == StockMovementType.Sale || type == StockMovementType.Return || type == StockMovementType.Cancellation || type == StockMovementType.Reservation) && !orderId.HasValue)
        {
            throw new ArgumentException("Order id is required for order stock movements.", nameof(orderId));
        }

        if ((type == StockMovementType.Sale || type == StockMovementType.Reservation) && newStock != previousStock - quantity)
        {
            throw new ArgumentException("Sale and reservation stock movements should decrease stock by quantity.", nameof(newStock));
        }

        if ((type == StockMovementType.Return || type == StockMovementType.Cancellation) && newStock != previousStock + quantity)
        {
            throw new ArgumentException("Return and cancellation stock movements should increase stock by quantity.", nameof(newStock));
        }

        if (type == StockMovementType.ManualAdjustment && newStock == previousStock)
        {
            throw new ArgumentException("Manual adjustment should change stock.", nameof(newStock));
        }
    }
}

using Sweet_Silvia_Domain.OrderItem;
using Sweet_Silvia_Domain.Product;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Tests;

public sealed class OrderPricingTests
{
    [Fact]
    public void OrderItem_keeps_the_price_snapshot_used_at_checkout()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            categoryId,
            null,
            "Polo Manuel",
            "Polo de prueba",
            Money.Create(40m),
            1);

        var item = OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            product.Name,
            "SKU-001",
            "M",
            "Negro",
            2,
            product.BasePrice,
            Money.Zero(),
            Money.Create(80m));

        product.Update(product.Name, product.Description, Money.Create(60m), categoryId, null);

        Assert.Equal(40m, item.HistoricalUnitPrice.Amount);
        Assert.Equal(80m, item.Subtotal.Amount);
        Assert.Equal("PEN", item.HistoricalUnitPrice.Currency);
    }
}

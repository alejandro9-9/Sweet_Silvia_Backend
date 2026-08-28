using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.ProductImage;

public sealed class ProductImage : Entity
{
    private ProductImage() { }

    public ProductImage(Guid id, Guid productId, Guid? productVariantId, string url, string? altText, int order, bool isMain, DateTimeOffset createdAt)
        : base(id)
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        Url = url;
        AltText = altText;
        Order = order;
        IsMain = isMain;
        CreatedAt = createdAt;
    }

    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int Order { get; private set; }
    public bool IsMain { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ProductImage Create(Guid productId, Guid? productVariantId, string url, string? altText, int order, bool isMain)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Product image url is required.", nameof(url));
        }

        if (order < 0)
        {
            throw new ArgumentException("Image order cannot be negative.", nameof(order));
        }

        return new ProductImage(Guid.NewGuid(), productId, productVariantId, url.Trim(), altText?.Trim(), order, isMain, DateTimeOffset.UtcNow);
    }
}

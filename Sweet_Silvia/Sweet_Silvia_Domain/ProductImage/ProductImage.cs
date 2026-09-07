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

        ValidateUrl(url);

        if (order < 0)
        {
            throw new ArgumentException("Image order cannot be negative.", nameof(order));
        }

        return new ProductImage(Guid.NewGuid(), productId, productVariantId, url.Trim(), altText?.Trim(), order, isMain, DateTimeOffset.UtcNow);
    }

    public void Update(Guid? productVariantId, string url, string? altText, int order, bool isMain)
    {
        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is invalid.", nameof(productVariantId));
        }

        ValidateUrl(url);

        if (order < 0)
        {
            throw new ArgumentException("Image order cannot be negative.", nameof(order));
        }

        ProductVariantId = productVariantId;
        Url = url.Trim();
        AltText = altText?.Trim();
        Order = order;
        IsMain = isMain;
    }

    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Product image url is required.", nameof(url));
        }

        url = url.Trim();
        var isLocalUpload = url.StartsWith("/uploads/", StringComparison.Ordinal);
        var isHttpUrl = Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        if (!isLocalUpload && !isHttpUrl)
        {
            throw new ArgumentException("Product image url should be an HTTP(S) URL or a local upload path.", nameof(url));
        }
    }
}

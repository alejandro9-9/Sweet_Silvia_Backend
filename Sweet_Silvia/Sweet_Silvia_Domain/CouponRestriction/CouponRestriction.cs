using Sweet_Silvia_Domain.Abstractions;

namespace Sweet_Silvia_Domain.CouponRestriction;

public sealed class CouponRestriction : Entity
{
    private CouponRestriction() { }

    public CouponRestriction(Guid id, Guid couponId, Guid? categoryId, Guid? productId, Guid? productVariantId, Guid? collectionId)
        : base(id)
    {
        CouponId = couponId;
        CategoryId = categoryId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        CollectionId = collectionId;
    }

    public Guid CouponId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public Guid? CollectionId { get; private set; }

    public static CouponRestriction Create(Guid couponId, Guid? categoryId = null, Guid? productId = null, Guid? productVariantId = null, Guid? collectionId = null)
    {
        if (couponId == Guid.Empty)
        {
            throw new ArgumentException("Coupon id is required.", nameof(couponId));
        }

        if (categoryId is null && productId is null && productVariantId is null && collectionId is null)
        {
            throw new ArgumentException("At least one coupon restriction target is required.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category id is invalid.", nameof(categoryId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is invalid.", nameof(productId));
        }

        if (productVariantId == Guid.Empty)
        {
            throw new ArgumentException("Product variant id is invalid.", nameof(productVariantId));
        }

        if (collectionId == Guid.Empty)
        {
            throw new ArgumentException("Collection id is invalid.", nameof(collectionId));
        }

        return new CouponRestriction(Guid.NewGuid(), couponId, categoryId, productId, productVariantId, collectionId);
    }
}

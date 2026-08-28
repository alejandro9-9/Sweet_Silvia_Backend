using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.Shared.ValueObjects;

namespace Sweet_Silvia_Domain.Coupon;

public sealed class Coupon : Entity
{
    private Coupon() { }

    public Coupon(Guid id, CouponCode code, string name, CouponScope scope, DiscountType discountType, decimal discountValue, DateTimeOffset startDate, DateTimeOffset endDate, bool isActive)
        : base(id)
    {
        Code = code;
        Name = name;
        Scope = scope;
        DiscountType = discountType;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }

    public CouponCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public CouponScope Scope { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumPurchaseAmount { get; private set; }
    public int? MinimumItemsQuantity { get; private set; }
    public int? MaximumDiscountedItemsQuantity { get; private set; }
    public CouponApplicationStrategy? ApplicationStrategy { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public int? MaximumTotalUses { get; private set; }
    public int? MaximumUsesPerUser { get; private set; }

    public static Coupon Create(CouponCode code, string name, CouponScope scope, DiscountType discountType, decimal discountValue, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (code is null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Coupon name is required.", nameof(name));
        }

        if (discountValue <= 0)
        {
            throw new ArgumentException("Discount value should be greater than zero.", nameof(discountValue));
        }

        if (discountType == DiscountType.Percentage && discountValue > 100)
        {
            throw new ArgumentException("Percentage discount cannot be greater than 100.", nameof(discountValue));
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException("Coupon end date should be greater than start date.", nameof(endDate));
        }

        var result = new Coupon(Guid.NewGuid(), code, name.Trim(), scope, discountType, discountValue, startDate, endDate, true);
        result.AddDomainEvent(new CouponCreatedEvent(result.Id));
        return result;
    }

    public void ConfigureRules(decimal? minimumPurchaseAmount, int? minimumItemsQuantity, int? maximumDiscountedItemsQuantity, CouponApplicationStrategy? applicationStrategy, int? maximumTotalUses, int? maximumUsesPerUser)
    {
        if (minimumPurchaseAmount < 0)
        {
            throw new ArgumentException("Minimum purchase amount cannot be negative.", nameof(minimumPurchaseAmount));
        }

        if (minimumItemsQuantity < 0)
        {
            throw new ArgumentException("Minimum items quantity cannot be negative.", nameof(minimumItemsQuantity));
        }

        if (maximumDiscountedItemsQuantity <= 0)
        {
            throw new ArgumentException("Maximum discounted items quantity should be greater than zero.", nameof(maximumDiscountedItemsQuantity));
        }

        if (maximumTotalUses <= 0)
        {
            throw new ArgumentException("Maximum total uses should be greater than zero.", nameof(maximumTotalUses));
        }

        if (maximumUsesPerUser <= 0)
        {
            throw new ArgumentException("Maximum uses per user should be greater than zero.", nameof(maximumUsesPerUser));
        }

        if (Scope == CouponScope.Order && maximumDiscountedItemsQuantity.HasValue)
        {
            throw new ArgumentException("Order coupons should not define maximum discounted items quantity.", nameof(maximumDiscountedItemsQuantity));
        }

        if (Scope == CouponScope.Item && maximumDiscountedItemsQuantity.HasValue && applicationStrategy is null)
        {
            throw new ArgumentException("Item coupons with limited quantity should define an application strategy.", nameof(applicationStrategy));
        }

        MinimumPurchaseAmount = minimumPurchaseAmount;
        MinimumItemsQuantity = minimumItemsQuantity;
        MaximumDiscountedItemsQuantity = maximumDiscountedItemsQuantity;
        ApplicationStrategy = applicationStrategy;
        MaximumTotalUses = maximumTotalUses;
        MaximumUsesPerUser = maximumUsesPerUser;
        AddDomainEvent(new CouponUpdatedEvent(Id));
    }

    public void Delete()
    {
        IsActive = false;
        AddDomainEvent(new CouponDeletedEvent(Id));
    }
}

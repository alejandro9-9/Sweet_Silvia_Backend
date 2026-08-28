using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponRestrictionEntity = Sweet_Silvia_Domain.CouponRestriction.CouponRestriction;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class CouponConfiguration : IEntityTypeConfiguration<CouponEntity>
{
    public void Configure(EntityTypeBuilder<CouponEntity> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Code).HasConversion(new CouponCodeConverter()).IsRequired().HasMaxLength(30);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(150);
        builder.Property(entity => entity.Scope).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.DiscountType).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.DiscountValue).HasPrecision(12, 2).IsRequired();
        builder.Property(entity => entity.MinimumPurchaseAmount).HasPrecision(12, 2);
        builder.Property(entity => entity.MinimumItemsQuantity);
        builder.Property(entity => entity.MaximumDiscountedItemsQuantity);
        builder.Property(entity => entity.ApplicationStrategy).HasConversion<string>().HasMaxLength(40);
        builder.Property(entity => entity.StartDate).IsRequired();
        builder.Property(entity => entity.EndDate).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.MaximumTotalUses);
        builder.Property(entity => entity.MaximumUsesPerUser);
        builder.HasIndex(entity => entity.Code).IsUnique();
        builder.HasIndex(entity => new { entity.IsActive, entity.StartDate, entity.EndDate });
    }
}

internal sealed class CouponRestrictionConfiguration : IEntityTypeConfiguration<CouponRestrictionEntity>
{
    public void Configure(EntityTypeBuilder<CouponRestrictionEntity> builder)
    {
        builder.ToTable("CouponRestrictions");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CouponId).IsRequired();
        builder.Property(entity => entity.CategoryId);
        builder.Property(entity => entity.ProductId);
        builder.Property(entity => entity.ProductVariantId);
        builder.Property(entity => entity.CollectionId);
        builder.HasIndex(entity => entity.CouponId);
        builder.HasIndex(entity => entity.CategoryId);
        builder.HasIndex(entity => entity.ProductId);
        builder.HasIndex(entity => entity.ProductVariantId);
        builder.HasIndex(entity => entity.CollectionId);
    }
}

internal sealed class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsageEntity>
{
    public void Configure(EntityTypeBuilder<CouponUsageEntity> builder)
    {
        builder.ToTable("CouponUsages");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CouponId).IsRequired();
        builder.Property(entity => entity.UserId).IsRequired();
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.Scope).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.OrderItemId);
        builder.Property(entity => entity.AppliedQuantity);
        builder.Property(entity => entity.DiscountAmount).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.UsedAt).IsRequired();
        builder.HasIndex(entity => new { entity.CouponId, entity.UserId });
        builder.HasIndex(entity => entity.OrderId);
    }
}

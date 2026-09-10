using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using CouponRestrictionEntity = Sweet_Silvia_Domain.CouponRestriction.CouponRestriction;
using CouponUsageEntity = Sweet_Silvia_Domain.CouponUsage.CouponUsage;
using CategoryEntity = Sweet_Silvia_Domain.Category.Category;
using CollectionEntity = Sweet_Silvia_Domain.Collection.Collection;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using ProductEntity = Sweet_Silvia_Domain.Product.Product;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using UserEntity = Sweet_Silvia_Domain.User.User;

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
        builder.HasOne<CouponEntity>().WithMany().HasForeignKey(entity => entity.CouponId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CategoryEntity>().WithMany().HasForeignKey(entity => entity.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductEntity>().WithMany().HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductVariantEntity>().WithMany().HasForeignKey(entity => entity.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CollectionEntity>().WithMany().HasForeignKey(entity => entity.CollectionId).OnDelete(DeleteBehavior.Restrict);
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
        builder.HasIndex(entity => new { entity.CouponId, entity.OrderId }).IsUnique();
        builder.HasOne<CouponEntity>().WithMany().HasForeignKey(entity => entity.CouponId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrderEntity>().WithMany().HasForeignKey(entity => entity.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrderItemEntity>().WithMany().HasForeignKey(entity => entity.OrderItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

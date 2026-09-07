using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CartAppliedCouponEntity = Sweet_Silvia_Domain.CartAppliedCoupon.CartAppliedCoupon;
using CartEntity = Sweet_Silvia_Domain.Cart.Cart;
using CartItemEntity = Sweet_Silvia_Domain.CartItem.CartItem;
using CouponEntity = Sweet_Silvia_Domain.Coupon.Coupon;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;
using UserEntity = Sweet_Silvia_Domain.User.User;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<CartEntity>
{
    public void Configure(EntityTypeBuilder<CartEntity> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.UserId).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Ignore(entity => entity.Items);
        builder.HasIndex(entity => new { entity.UserId, entity.IsActive });
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItemEntity>
{
    public void Configure(EntityTypeBuilder<CartItemEntity> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CartId).IsRequired();
        builder.Property(entity => entity.ProductVariantId).IsRequired();
        builder.Property(entity => entity.Quantity).IsRequired();
        builder.Property(entity => entity.AddedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.HasIndex(entity => new { entity.CartId, entity.ProductVariantId }).IsUnique();
        builder.HasOne<CartEntity>().WithMany().HasForeignKey(entity => entity.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductVariantEntity>().WithMany().HasForeignKey(entity => entity.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CartAppliedCouponConfiguration : IEntityTypeConfiguration<CartAppliedCouponEntity>
{
    public void Configure(EntityTypeBuilder<CartAppliedCouponEntity> builder)
    {
        builder.ToTable("CartAppliedCoupons");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CartId).IsRequired();
        builder.Property(entity => entity.CouponId).IsRequired();
        builder.Property(entity => entity.Scope).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.CartItemId);
        builder.Property(entity => entity.AppliedQuantity);
        builder.Property(entity => entity.EstimatedDiscountAmount).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.AppliedAt).IsRequired();
        builder.HasIndex(entity => entity.CartId);
        builder.HasIndex(entity => entity.CouponId);
        builder.HasOne<CartEntity>().WithMany().HasForeignKey(entity => entity.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CouponEntity>().WithMany().HasForeignKey(entity => entity.CouponId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CartItemEntity>().WithMany().HasForeignKey(entity => entity.CartItemId).OnDelete(DeleteBehavior.SetNull);
    }
}

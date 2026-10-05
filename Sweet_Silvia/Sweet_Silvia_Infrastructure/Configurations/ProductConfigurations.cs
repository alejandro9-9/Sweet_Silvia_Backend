using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CategoryEntity = Sweet_Silvia_Domain.Category.Category;
using CollectionEntity = Sweet_Silvia_Domain.Collection.Collection;
using ProductEntity = Sweet_Silvia_Domain.Product.Product;
using ProductImageEntity = Sweet_Silvia_Domain.ProductImage.ProductImage;
using ProductVariantEntity = Sweet_Silvia_Domain.ProductVariant.ProductVariant;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CategoryId).IsRequired();
        builder.Property(entity => entity.CollectionId);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(180);
        builder.Property(entity => entity.Description).HasMaxLength(1000);
        builder.Property(entity => entity.BasePrice).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.DisplayOrder).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.HasIndex(entity => entity.CategoryId);
        builder.HasIndex(entity => entity.CollectionId);
        builder.HasOne<CategoryEntity>().WithMany().HasForeignKey(entity => entity.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CollectionEntity>().WithMany().HasForeignKey(entity => entity.CollectionId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariantEntity>
{
    public void Configure(EntityTypeBuilder<ProductVariantEntity> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ProductId).IsRequired();
        builder.Property(entity => entity.Size).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Color).IsRequired().HasMaxLength(80);
        builder.Property(entity => entity.Sku).HasConversion(new SkuConverter()).IsRequired().HasMaxLength(50);
        builder.Property(entity => entity.PhysicalStock).IsRequired();
        builder.Property(entity => entity.Price).HasConversion(new NullableMoneyConverter()).HasMaxLength(40);
        builder.Property(entity => entity.ShippingWeightKg).HasPrecision(10, 3).IsRequired();
        builder.Property(entity => entity.ShippingLengthCm).HasPrecision(10, 2).IsRequired();
        builder.Property(entity => entity.ShippingWidthCm).HasPrecision(10, 2).IsRequired();
        builder.Property(entity => entity.ShippingHeightCm).HasPrecision(10, 2).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.HasIndex(entity => entity.ProductId);
        builder.HasIndex(entity => entity.Sku).IsUnique();
        builder.HasOne<ProductEntity>().WithMany().HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Property<uint>("xmin").IsRowVersion();
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageEntity>
{
    public void Configure(EntityTypeBuilder<ProductImageEntity> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ProductId).IsRequired();
        builder.Property(entity => entity.ProductVariantId);
        builder.Property(entity => entity.Url).IsRequired().HasMaxLength(1000);
        builder.Property(entity => entity.AltText).HasMaxLength(250);
        builder.Property(entity => entity.Order).IsRequired();
        builder.Property(entity => entity.IsMain).IsRequired();
        builder.HasIndex(entity => entity.ProductId);
        builder.HasIndex(entity => entity.ProductVariantId);
        builder.HasOne<ProductEntity>().WithMany().HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductVariantEntity>().WithMany().HasForeignKey(entity => entity.ProductVariantId).OnDelete(DeleteBehavior.SetNull);
    }
}

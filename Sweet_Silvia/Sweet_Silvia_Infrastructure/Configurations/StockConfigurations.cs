using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMovementEntity = Sweet_Silvia_Domain.StockMovement.StockMovement;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovementEntity>
{
    public void Configure(EntityTypeBuilder<StockMovementEntity> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ProductVariantId).IsRequired();
        builder.Property(entity => entity.OrderId);
        builder.Property(entity => entity.Type).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Quantity).IsRequired();
        builder.Property(entity => entity.PreviousStock).IsRequired();
        builder.Property(entity => entity.NewStock).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.ProductVariantId);
        builder.HasIndex(entity => entity.OrderId);
        builder.HasIndex(entity => entity.Type);
    }
}

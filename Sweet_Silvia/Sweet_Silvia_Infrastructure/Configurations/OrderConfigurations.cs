using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderEntity = Sweet_Silvia_Domain.Order.Order;
using OrderItemEntity = Sweet_Silvia_Domain.OrderItem.OrderItem;
using OrderStatusHistoryEntity = Sweet_Silvia_Domain.OrderStatusHistory.OrderStatusHistory;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.UserId).IsRequired();
        builder.Property(entity => entity.AddressId).IsRequired();
        builder.Property(entity => entity.CouponId);
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.Property(entity => entity.Subtotal).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.DiscountTotal).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.ShippingCost).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Total).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Ignore(entity => entity.Items);
        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => entity.AddressId);
        builder.HasIndex(entity => entity.CouponId);
        builder.HasIndex(entity => entity.Status);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemEntity>
{
    public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.ProductVariantId).IsRequired();
        builder.Property(entity => entity.HistoricalProductName).IsRequired().HasMaxLength(180);
        builder.Property(entity => entity.HistoricalSku).IsRequired().HasMaxLength(50);
        builder.Property(entity => entity.HistoricalSize).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.HistoricalColor).IsRequired().HasMaxLength(80);
        builder.Property(entity => entity.Quantity).IsRequired();
        builder.Property(entity => entity.HistoricalUnitPrice).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.HistoricalUnitDiscount).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Subtotal).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.HasIndex(entity => entity.OrderId);
        builder.HasIndex(entity => entity.ProductVariantId);
    }
}

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistoryEntity>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistoryEntity> builder)
    {
        builder.ToTable("OrderStatusHistories");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.PreviousStatus).HasConversion<string>().HasMaxLength(40);
        builder.Property(entity => entity.NewStatus).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.ChangedByUserId);
        builder.Property(entity => entity.ChangedAt).IsRequired();
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.OrderId);
    }
}

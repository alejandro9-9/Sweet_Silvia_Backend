using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipmentEntity = Sweet_Silvia_Domain.Shipment.Shipment;
using ShipmentEventEntity = Sweet_Silvia_Domain.ShipmentEvent.ShipmentEvent;
using ShippingRateEntity = Sweet_Silvia_Domain.ShippingRate.ShippingRate;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRateEntity>
{
    public void Configure(EntityTypeBuilder<ShippingRateEntity> builder)
    {
        builder.ToTable("ShippingRates");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CourierId).IsRequired();
        builder.Property(entity => entity.CourierType).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Country).IsRequired().HasMaxLength(2);
        builder.Property(entity => entity.DepartmentId).IsRequired();
        builder.Property(entity => entity.ProvinceId);
        builder.Property(entity => entity.DistrictId);
        builder.Property(entity => entity.DestinationType).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.ServiceType).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Cost).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.FreeShippingMinimumAmount).HasConversion(new NullableMoneyConverter()).HasMaxLength(40);
        builder.Property(entity => entity.EstimatedTime).HasMaxLength(80);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.HasIndex(entity => new { entity.DepartmentId, entity.ProvinceId, entity.DistrictId });
        builder.HasIndex(entity => new { entity.DestinationType, entity.ServiceType, entity.IsActive });
    }
}

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentEntity> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.CourierId).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.TrackingCode).HasMaxLength(120);
        builder.Property(entity => entity.ExternalShipmentCode).HasMaxLength(120);
        builder.Property(entity => entity.ShippingCost).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.RegisteredExternallyAt);
        builder.Property(entity => entity.EstimatedDeliveryAt);
        builder.Property(entity => entity.DeliveredAt);
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.OrderId);
        builder.HasIndex(entity => entity.CourierId);
        builder.HasIndex(entity => entity.Status);
        builder.HasIndex(entity => entity.TrackingCode);
    }
}

internal sealed class ShipmentEventConfiguration : IEntityTypeConfiguration<ShipmentEventEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentEventEntity> builder)
    {
        builder.ToTable("ShipmentEvents");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ShipmentId).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Description).IsRequired().HasMaxLength(500);
        builder.Property(entity => entity.Location).HasMaxLength(180);
        builder.Property(entity => entity.EventDate).IsRequired();
        builder.HasIndex(entity => entity.ShipmentId);
    }
}

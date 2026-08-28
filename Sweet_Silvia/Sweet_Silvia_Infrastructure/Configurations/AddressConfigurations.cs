using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AddressEntity = Sweet_Silvia_Domain.Address.Address;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.UserId).IsRequired();
        builder.Property(entity => entity.ReceiverName).IsRequired().HasMaxLength(180);
        builder.Property(entity => entity.ReceiverPhone).HasConversion(new PhoneNumberConverter()).IsRequired().HasMaxLength(12);
        builder.Property(entity => entity.Country).IsRequired().HasMaxLength(2);
        builder.Property(entity => entity.DepartmentId).IsRequired();
        builder.Property(entity => entity.ProvinceId).IsRequired();
        builder.Property(entity => entity.DistrictId).IsRequired();
        builder.Property(entity => entity.Line).HasConversion(new AddressLineConverter()).IsRequired().HasMaxLength(250);
        builder.Property(entity => entity.References).HasMaxLength(500);
        builder.Property(entity => entity.IsDefault).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => new { entity.DepartmentId, entity.ProvinceId, entity.DistrictId });
    }
}

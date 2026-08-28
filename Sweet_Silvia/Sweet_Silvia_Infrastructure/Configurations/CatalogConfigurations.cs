using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CategoryEntity = Sweet_Silvia_Domain.Category.Category;
using CollectionEntity = Sweet_Silvia_Domain.Collection.Collection;
using CourierEntity = Sweet_Silvia_Domain.Courier.Courier;
using DepartmentEntity = Sweet_Silvia_Domain.Department.Department;
using DistrictEntity = Sweet_Silvia_Domain.District.District;
using PaymentMethodEntity = Sweet_Silvia_Domain.PaymentMethod.PaymentMethod;
using ProvinceEntity = Sweet_Silvia_Domain.Province.Province;
using RoleEntity = Sweet_Silvia_Domain.Role.Role;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(150);
        builder.Property(entity => entity.Description).IsRequired().HasMaxLength(500);
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}

internal sealed class CollectionConfiguration : IEntityTypeConfiguration<CollectionEntity>
{
    public void Configure(EntityTypeBuilder<CollectionEntity> builder)
    {
        builder.ToTable("Collections");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(150);
        builder.Property(entity => entity.Description).IsRequired().HasMaxLength(500);
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}

internal sealed class CourierConfiguration : IEntityTypeConfiguration<CourierEntity>
{
    public void Configure(EntityTypeBuilder<CourierEntity> builder)
    {
        builder.ToTable("Couriers");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(150);
        builder.Property(entity => entity.Type).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.RequiresExternalIntegration).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<DepartmentEntity>
{
    public void Configure(EntityTypeBuilder<DepartmentEntity> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.DestinationType).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}

internal sealed class ProvinceConfiguration : IEntityTypeConfiguration<ProvinceEntity>
{
    public void Configure(EntityTypeBuilder<ProvinceEntity> builder)
    {
        builder.ToTable("Provinces");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.DepartmentId).IsRequired();
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.HasIndex(entity => new { entity.DepartmentId, entity.Name }).IsUnique();
    }
}

internal sealed class DistrictConfiguration : IEntityTypeConfiguration<DistrictEntity>
{
    public void Configure(EntityTypeBuilder<DistrictEntity> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ProvinceId).IsRequired();
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.Ubigeo).HasMaxLength(10);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.HasIndex(entity => new { entity.ProvinceId, entity.Name }).IsUnique();
    }
}

internal sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethodEntity>
{
    public void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.Type).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.RequiresManualVerification).IsRequired();
        builder.Property(entity => entity.RequiresExternalIntegration).IsRequired();
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(80);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.HasIndex(entity => entity.Name).IsUnique();
    }
}

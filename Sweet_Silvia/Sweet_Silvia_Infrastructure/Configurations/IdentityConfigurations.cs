using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefreshTokenEntity = Sweet_Silvia_Domain.RefreshToken.RefreshToken;
using RoleEntity = Sweet_Silvia_Domain.Role.Role;
using UserEntity = Sweet_Silvia_Domain.User.User;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.RoleId).IsRequired();
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.PaternalSurname).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.MaternalSurname).HasMaxLength(120);
        builder.Property(entity => entity.Phone).HasConversion(new NullablePhoneNumberConverter()).IsRequired(false).HasMaxLength(12);
        builder.Property(entity => entity.Email).HasConversion(new EmailAddressConverter()).IsRequired().HasMaxLength(255);
        builder.Property(entity => entity.PasswordHash).HasMaxLength(512);
        builder.Property(entity => entity.GoogleSubject).HasMaxLength(255);
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.Ignore(entity => entity.IsActive);
        builder.HasIndex(entity => entity.Email).IsUnique();
        builder.HasIndex(entity => entity.GoogleSubject).IsUnique().HasFilter("\"GoogleSubject\" IS NOT NULL");
        builder.HasOne<RoleEntity>().WithMany().HasForeignKey(entity => entity.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.UserId).IsRequired();
        builder.Property(entity => entity.TokenHash).IsRequired().HasMaxLength(512);
        builder.Property(entity => entity.ExpiresAt).IsRequired();
        builder.Property(entity => entity.RevokedAt);
        builder.Property(entity => entity.DeviceInfo).HasMaxLength(500);
        builder.Property(entity => entity.IpAddress).HasMaxLength(64);
        builder.Property(entity => entity.UserAgent).HasMaxLength(500);
        builder.Property(entity => entity.LastUsedAt);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Ignore(entity => entity.IsRevoked);
        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasIndex(entity => entity.UserId);
        builder.HasOne<UserEntity>().WithMany().HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

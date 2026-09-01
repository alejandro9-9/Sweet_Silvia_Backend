using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuditLogEntity = Sweet_Silvia_Domain.AuditLog.AuditLog;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ActorRole).HasMaxLength(80);
        builder.Property(entity => entity.Action).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.EntityName).IsRequired().HasMaxLength(120);
        builder.Property(entity => entity.OccurredAt).IsRequired();
        builder.Property(entity => entity.IpAddress).HasMaxLength(64);
        builder.Property(entity => entity.UserAgent).HasMaxLength(500);
        builder.Property(entity => entity.OldValuesJson).HasColumnType("jsonb");
        builder.Property(entity => entity.NewValuesJson).HasColumnType("jsonb");
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.ActorUserId);
        builder.HasIndex(entity => new { entity.EntityName, entity.EntityId });
        builder.HasIndex(entity => entity.OccurredAt);
    }
}

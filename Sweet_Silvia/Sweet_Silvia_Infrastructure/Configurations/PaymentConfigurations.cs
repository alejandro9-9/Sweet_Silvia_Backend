using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentEntity = Sweet_Silvia_Domain.Payment.Payment;
using PaymentReceiptEntity = Sweet_Silvia_Domain.PaymentReceipt.PaymentReceipt;
using PaymentReviewEntity = Sweet_Silvia_Domain.PaymentReview.PaymentReview;

namespace Sweet_Silvia_Infrastructure.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.OrderId).IsRequired();
        builder.Property(entity => entity.PaymentMethodId).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Amount).HasConversion(new MoneyConverter()).IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Currency).IsRequired().HasMaxLength(3);
        builder.Property(entity => entity.TransactionCode).HasMaxLength(120);
        builder.Property(entity => entity.AuthorizationCode).HasMaxLength(120);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.PaidAt);
        builder.Property(entity => entity.ReceiptUploadedAt);
        builder.Property(entity => entity.ReviewedAt);
        builder.Property(entity => entity.ReviewedByUserId);
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.OrderId);
        builder.HasIndex(entity => entity.PaymentMethodId);
        builder.HasIndex(entity => entity.Status);
    }
}

internal sealed class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceiptEntity>
{
    public void Configure(EntityTypeBuilder<PaymentReceiptEntity> builder)
    {
        builder.ToTable("PaymentReceipts");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.PaymentId).IsRequired();
        builder.Property(entity => entity.FileUrl).IsRequired().HasMaxLength(1000);
        builder.Property(entity => entity.OperationCode).HasMaxLength(120);
        builder.Property(entity => entity.DeclaredAmount).HasConversion(new NullableMoneyConverter()).HasMaxLength(40);
        builder.Property(entity => entity.UploadedAt).IsRequired();
        builder.Property(entity => entity.Status).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.PaymentId);
    }
}

internal sealed class PaymentReviewConfiguration : IEntityTypeConfiguration<PaymentReviewEntity>
{
    public void Configure(EntityTypeBuilder<PaymentReviewEntity> builder)
    {
        builder.ToTable("PaymentReviews");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.PaymentId).IsRequired();
        builder.Property(entity => entity.ReviewerUserId).IsRequired();
        builder.Property(entity => entity.Result).HasConversion<string>().IsRequired().HasMaxLength(40);
        builder.Property(entity => entity.ReviewedAt).IsRequired();
        builder.Property(entity => entity.Observation).HasMaxLength(500);
        builder.HasIndex(entity => entity.PaymentId);
        builder.HasIndex(entity => entity.ReviewerUserId);
    }
}

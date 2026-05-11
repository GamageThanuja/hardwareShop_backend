using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasKey(p => p.Id);

        b.Property(p => p.Amount).HasPrecision(18, 4).IsRequired();
        b.Property(p => p.Method).IsRequired();
        b.Property(p => p.PaymentDate).IsRequired();
        b.Property(p => p.ReferenceNumber).HasMaxLength(100);
        b.Property(p => p.Notes).HasMaxLength(1000);
        b.Property(p => p.VoidReason).HasMaxLength(500);

        b.HasIndex(p => p.SalesOrderId).HasDatabaseName("IX_Payments_SalesOrderId");
        b.HasIndex(p => p.PaymentDate).HasDatabaseName("IX_Payments_PaymentDate");

        b.HasOne(p => p.SalesOrder)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> b)
    {
        b.ToTable("SalesOrders");
        b.HasKey(o => o.Id);

        b.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        b.Property(o => o.SubTotal).HasPrecision(18, 4);
        b.Property(o => o.TaxAmount).HasPrecision(18, 4);
        b.Property(o => o.DiscountAmount).HasPrecision(18, 4);
        b.Property(o => o.GrandTotal).HasPrecision(18, 4);
        b.Property(o => o.Notes).HasMaxLength(1000);
        b.Property(o => o.Status).IsRequired();

        b.HasIndex(o => o.OrderNumber).IsUnique().HasDatabaseName("IX_SalesOrders_OrderNumber");
        b.HasIndex(o => o.CustomerId).HasDatabaseName("IX_SalesOrders_CustomerId");
        b.HasIndex(o => o.Status).HasDatabaseName("IX_SalesOrders_Status");
        b.HasIndex(o => o.OrderDate).HasDatabaseName("IX_SalesOrders_OrderDate");
        b.HasQueryFilter(o => !o.IsDeleted);

        b.HasOne(o => o.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

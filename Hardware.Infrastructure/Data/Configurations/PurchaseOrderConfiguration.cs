using Hardware.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("PurchaseOrders");
        b.HasKey(o => o.Id);

        b.Property(o => o.PONumber).IsRequired().HasMaxLength(30);
        b.Property(o => o.TotalAmount).HasPrecision(18, 4);
        b.Property(o => o.Notes).HasMaxLength(1000);
        b.Property(o => o.Status).IsRequired();

        b.HasIndex(o => o.PONumber).IsUnique().HasDatabaseName("IX_PurchaseOrders_PONumber");
        b.HasIndex(o => o.SupplierId).HasDatabaseName("IX_PurchaseOrders_SupplierId");
        b.HasIndex(o => o.Status).HasDatabaseName("IX_PurchaseOrders_Status");
        b.HasIndex(o => o.OrderDate).HasDatabaseName("IX_PurchaseOrders_OrderDate");
        b.HasQueryFilter(o => !o.IsDeleted);

        b.HasOne(o => o.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Hardware.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> b)
    {
        b.ToTable("PurchaseReturns");
        b.HasKey(r => r.Id);

        b.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(30);
        b.Property(r => r.Reason).HasMaxLength(500);
        b.Property(r => r.Notes).HasMaxLength(1000);
        b.Property(r => r.Status).IsRequired();

        b.HasIndex(r => r.ReturnNumber).IsUnique().HasDatabaseName("IX_PurchaseReturns_ReturnNumber");
        b.HasIndex(r => r.PurchaseOrderId).HasDatabaseName("IX_PurchaseReturns_PurchaseOrderId");
        b.HasQueryFilter(r => !r.IsDeleted);

        b.HasOne(r => r.PurchaseOrder)
            .WithMany()
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

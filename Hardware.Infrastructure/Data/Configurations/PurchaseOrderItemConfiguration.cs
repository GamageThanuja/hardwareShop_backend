using Hardware.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> b)
    {
        b.ToTable("PurchaseOrderItems");
        b.HasKey(i => i.Id);

        b.Property(i => i.UnitCost).HasPrecision(18, 4);
        b.Property(i => i.SubTotal).HasPrecision(18, 4);

        b.HasIndex(i => i.PurchaseOrderId).HasDatabaseName("IX_PurchaseOrderItems_PurchaseOrderId");
        b.HasIndex(i => i.ProductId).HasDatabaseName("IX_PurchaseOrderItems_ProductId");
        b.HasQueryFilter(i => !i.IsDeleted);

        b.HasOne(i => i.PurchaseOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

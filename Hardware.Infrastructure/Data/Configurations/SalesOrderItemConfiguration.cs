using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> b)
    {
        b.ToTable("SalesOrderItems");
        b.HasKey(i => i.Id);

        b.Property(i => i.UnitPrice).HasPrecision(18, 4);
        b.Property(i => i.DiscountPercent).HasPrecision(5, 2);
        b.Property(i => i.TaxPercent).HasPrecision(5, 2);
        b.Property(i => i.SubTotal).HasPrecision(18, 4);

        b.HasIndex(i => i.SalesOrderId).HasDatabaseName("IX_SalesOrderItems_SalesOrderId");
        b.HasIndex(i => i.ProductId).HasDatabaseName("IX_SalesOrderItems_ProductId");
        b.HasQueryFilter(i => !i.IsDeleted);

        b.HasOne(i => i.SalesOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.Product)
            .WithMany(p => p.SalesOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

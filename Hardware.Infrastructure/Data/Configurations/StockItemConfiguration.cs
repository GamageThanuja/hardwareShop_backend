using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> b)
    {
        b.ToTable("StockItems");
        b.HasKey(s => s.Id);

        b.Ignore(s => s.QuantityAvailable);

        b.HasIndex(s => new { s.ProductId, s.WarehouseId })
            .IsUnique()
            .HasDatabaseName("IX_StockItems_ProductId_WarehouseId");

        b.HasQueryFilter(s => !s.IsDeleted);

        b.HasOne(s => s.Product)
            .WithMany(p => p.StockItems)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(s => s.Warehouse)
            .WithMany(w => w.StockItems)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

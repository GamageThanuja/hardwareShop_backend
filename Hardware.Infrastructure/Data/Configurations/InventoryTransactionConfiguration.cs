using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> b)
    {
        b.ToTable("InventoryTransactions");
        b.HasKey(t => t.Id);

        b.Property(t => t.TransactionType).IsRequired();
        b.Property(t => t.ReferenceType).HasMaxLength(50);
        b.Property(t => t.Notes).HasMaxLength(500);

        b.HasIndex(t => t.ProductId).HasDatabaseName("IX_InventoryTransactions_ProductId");
        b.HasIndex(t => t.WarehouseId).HasDatabaseName("IX_InventoryTransactions_WarehouseId");
        b.HasIndex(t => t.TransactionType).HasDatabaseName("IX_InventoryTransactions_TransactionType");
        b.HasIndex(t => new { t.ReferenceType, t.ReferenceId }).HasDatabaseName("IX_InventoryTransactions_Reference");
        b.HasIndex(t => t.CreatedAt).HasDatabaseName("IX_InventoryTransactions_CreatedAt");

        b.HasQueryFilter(t => !t.IsDeleted);

        b.HasOne(t => t.Product)
            .WithMany(p => p.InventoryTransactions)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(t => t.Warehouse)
            .WithMany(w => w.InventoryTransactions)
            .HasForeignKey(t => t.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(p => p.Id);

        b.Property(p => p.SKU).IsRequired().HasMaxLength(50);
        b.Property(p => p.Name).IsRequired().HasMaxLength(200);
        b.Property(p => p.Description).HasMaxLength(1000);
        b.Property(p => p.Barcode).HasMaxLength(50);
        b.Property(p => p.UnitPrice).HasPrecision(18, 4);
        b.Property(p => p.CostPrice).HasPrecision(18, 4);
        b.Property(p => p.Unit).IsRequired();
        b.Property(p => p.Status).IsRequired();

        b.HasIndex(p => p.SKU).IsUnique().HasDatabaseName("IX_Products_SKU");
        b.HasIndex(p => p.Barcode).HasDatabaseName("IX_Products_Barcode");
        b.HasIndex(p => p.CategoryId).HasDatabaseName("IX_Products_CategoryId");
        b.HasIndex(p => p.Status).HasDatabaseName("IX_Products_Status");
        b.HasQueryFilter(p => !p.IsDeleted);

        b.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

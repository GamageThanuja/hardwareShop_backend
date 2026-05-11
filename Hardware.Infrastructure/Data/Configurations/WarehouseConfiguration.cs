using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> b)
    {
        b.ToTable("Warehouses");
        b.HasKey(w => w.Id);

        b.Property(w => w.Name).IsRequired().HasMaxLength(100);
        b.Property(w => w.Address).HasMaxLength(300);
        b.Property(w => w.City).HasMaxLength(100);
        b.Property(w => w.Country).HasMaxLength(100);
        b.Property(w => w.Status).IsRequired();

        b.HasIndex(w => w.Status).HasDatabaseName("IX_Warehouses_Status");
        b.HasQueryFilter(w => !w.IsDeleted);
    }
}

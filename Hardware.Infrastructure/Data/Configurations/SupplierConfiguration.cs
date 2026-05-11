using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers");
        b.HasKey(s => s.Id);

        b.Property(s => s.Name).IsRequired().HasMaxLength(200);
        b.Property(s => s.ContactName).HasMaxLength(100);
        b.Property(s => s.Email).HasMaxLength(200);
        b.Property(s => s.Phone).HasMaxLength(30);
        b.Property(s => s.Address).HasMaxLength(300);
        b.Property(s => s.City).HasMaxLength(100);
        b.Property(s => s.Country).HasMaxLength(100);
        b.Property(s => s.Status).IsRequired();

        b.HasIndex(s => s.Name).HasDatabaseName("IX_Suppliers_Name");
        b.HasIndex(s => s.Status).HasDatabaseName("IX_Suppliers_Status");
        b.HasQueryFilter(s => !s.IsDeleted);
    }
}

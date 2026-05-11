using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers");
        b.HasKey(c => c.Id);

        b.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        b.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        b.Property(c => c.Email).HasMaxLength(200);
        b.Property(c => c.Phone).HasMaxLength(30);
        b.Property(c => c.Address).HasMaxLength(300);
        b.Property(c => c.City).HasMaxLength(100);
        b.Property(c => c.Country).HasMaxLength(100);
        b.Property(c => c.CreditLimit).HasPrecision(18, 4);
        b.Property(c => c.CustomerType).IsRequired();
        b.Property(c => c.Status).IsRequired();

        b.HasIndex(c => c.Email).HasDatabaseName("IX_Customers_Email");
        b.HasIndex(c => c.Status).HasDatabaseName("IX_Customers_Status");
        b.HasQueryFilter(c => !c.IsDeleted);
    }
}

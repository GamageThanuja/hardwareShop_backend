using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> b)
    {
        b.ToTable("SalesReturns");
        b.HasKey(r => r.Id);

        b.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(30);
        b.Property(r => r.Reason).HasMaxLength(500);
        b.Property(r => r.Notes).HasMaxLength(1000);
        b.Property(r => r.Status).IsRequired();

        b.HasIndex(r => r.ReturnNumber).IsUnique().HasDatabaseName("IX_SalesReturns_ReturnNumber");
        b.HasIndex(r => r.SalesOrderId).HasDatabaseName("IX_SalesReturns_SalesOrderId");
        b.HasQueryFilter(r => !r.IsDeleted);

        b.HasOne(r => r.SalesOrder)
            .WithMany()
            .HasForeignKey(r => r.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

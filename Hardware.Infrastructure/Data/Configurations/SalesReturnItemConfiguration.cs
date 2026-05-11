using Hardware.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
{
    public void Configure(EntityTypeBuilder<SalesReturnItem> b)
    {
        b.ToTable("SalesReturnItems");
        b.HasKey(i => i.Id);

        b.Property(i => i.Notes).HasMaxLength(500);

        b.HasOne(i => i.SalesReturn)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Hardware.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> b)
    {
        b.ToTable("PurchaseReturnItems");
        b.HasKey(i => i.Id);

        b.Property(i => i.UnitCost).HasPrecision(18, 4);
        b.Property(i => i.Notes).HasMaxLength(500);

        b.HasOne(i => i.PurchaseReturn)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

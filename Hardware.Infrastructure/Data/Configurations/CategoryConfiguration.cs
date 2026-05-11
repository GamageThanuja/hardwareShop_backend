using Hardware.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hardware.Infrastructure.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(c => c.Id);

        b.Property(c => c.Name).IsRequired().HasMaxLength(100);
        b.Property(c => c.Description).HasMaxLength(500);
        b.Property(c => c.Status).IsRequired();

        b.HasIndex(c => c.Name).HasDatabaseName("IX_Categories_Name");
        b.HasIndex(c => c.Status).HasDatabaseName("IX_Categories_Status");
        b.HasQueryFilter(c => !c.IsDeleted);

        b.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

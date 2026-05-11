using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Inventory;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}

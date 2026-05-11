using Hardware.Domain.Common;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Inventory;

public class Supplier : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    public ICollection<Product> Products { get; set; } = [];
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}

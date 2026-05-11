using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;

namespace Hardware.Domain.Entities.Purchasing;

public class PurchaseReturnItem : BaseEntity
{
    public Guid PurchaseReturnId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }

    public PurchaseReturn PurchaseReturn { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

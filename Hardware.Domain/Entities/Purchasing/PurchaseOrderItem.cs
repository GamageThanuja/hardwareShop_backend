using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;

namespace Hardware.Domain.Entities.Purchasing;

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal SubTotal { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

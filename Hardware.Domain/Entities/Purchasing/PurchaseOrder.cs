using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Purchasing;

public class PurchaseOrder : AuditableEntity
{
    public string PONumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
}

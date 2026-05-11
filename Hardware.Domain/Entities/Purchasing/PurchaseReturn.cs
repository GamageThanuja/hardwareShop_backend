using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Purchasing;

public class PurchaseReturn : AuditableEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid PurchaseOrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public PurchaseReturnStatus Status { get; set; } = PurchaseReturnStatus.Completed;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<PurchaseReturnItem> Items { get; set; } = [];
}

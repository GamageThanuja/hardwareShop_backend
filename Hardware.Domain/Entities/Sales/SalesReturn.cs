using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Sales;

public class SalesReturn : AuditableEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public SalesReturnStatus Status { get; set; } = SalesReturnStatus.Completed;

    public SalesOrder SalesOrder { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<SalesReturnItem> Items { get; set; } = [];
}

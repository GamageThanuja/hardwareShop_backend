using Hardware.Domain.Common;
using Hardware.Domain.Entities.Purchasing;
using Hardware.Domain.Entities.Sales;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Inventory;

public class Product : AuditableEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Piece;
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<StockItem> StockItems { get; set; } = [];
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = [];
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];
}

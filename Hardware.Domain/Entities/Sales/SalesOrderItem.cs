using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;

namespace Hardware.Domain.Entities.Sales;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal SubTotal { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

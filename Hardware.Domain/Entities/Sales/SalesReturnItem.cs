using Hardware.Domain.Common;
using Hardware.Domain.Entities.Inventory;

namespace Hardware.Domain.Entities.Sales;

public class SalesReturnItem : BaseEntity
{
    public Guid SalesReturnId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public SalesReturn SalesReturn { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Sales;

public class SalesOrder : AuditableEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public Guid? ProcessedByUserId { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal AmountPaid { get; set; }
    public decimal Balance => GrandTotal - AmountPaid;

    public Customer? Customer { get; set; }
    public ICollection<SalesOrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}

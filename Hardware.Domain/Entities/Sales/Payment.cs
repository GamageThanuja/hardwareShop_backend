using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Sales;

public class Payment : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public bool IsVoided { get; set; }
    public DateTime? VoidedAt { get; set; }
    public Guid? VoidedByUserId { get; set; }
    public string? VoidReason { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
}

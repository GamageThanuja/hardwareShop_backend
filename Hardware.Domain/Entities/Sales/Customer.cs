using Hardware.Domain.Common;
using Hardware.Domain.Enums;

namespace Hardware.Domain.Entities.Sales;

public class Customer : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Retail;
    public decimal CreditLimit { get; set; }
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
}

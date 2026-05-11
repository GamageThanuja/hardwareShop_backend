using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record UpdateSalesOrderStatusDto
{
    public SalesOrderStatus Status { get; init; }
}

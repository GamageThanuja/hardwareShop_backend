using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record SalesOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public DateTime OrderDate { get; init; }
    public SalesOrderStatus Status { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal GrandTotal { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<SalesOrderItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

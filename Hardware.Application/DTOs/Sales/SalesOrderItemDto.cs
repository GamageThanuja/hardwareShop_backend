namespace Hardware.Application.DTOs.Sales;

public sealed record SalesOrderItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSKU { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxPercent { get; init; }
    public decimal SubTotal { get; init; }
}

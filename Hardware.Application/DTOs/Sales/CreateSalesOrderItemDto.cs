namespace Hardware.Application.DTOs.Sales;

public sealed record CreateSalesOrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal TaxPercent { get; init; }
}

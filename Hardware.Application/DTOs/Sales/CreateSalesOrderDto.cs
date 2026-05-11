namespace Hardware.Application.DTOs.Sales;

public sealed record CreateSalesOrderDto
{
    public Guid? CustomerId { get; init; }
    public DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public string? Notes { get; init; }
    public IReadOnlyList<CreateSalesOrderItemDto> Items { get; init; } = [];
}

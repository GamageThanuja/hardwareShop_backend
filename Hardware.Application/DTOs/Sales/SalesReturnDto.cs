using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Sales;

public sealed record SalesReturnDto
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = string.Empty;
    public Guid SalesOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public DateTime ReturnDate { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public SalesReturnStatus Status { get; init; }
    public IReadOnlyList<SalesReturnItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public sealed record SalesReturnItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSKU { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

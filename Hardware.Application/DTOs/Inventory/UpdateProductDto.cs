using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record UpdateProductDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Barcode { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? SupplierId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal CostPrice { get; init; }
    public UnitOfMeasure Unit { get; init; }
    public int ReorderLevel { get; init; }
    public int ReorderQuantity { get; init; }
    public CommonStatus Status { get; init; }
}

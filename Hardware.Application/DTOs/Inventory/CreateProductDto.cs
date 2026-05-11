using Hardware.Domain.Enums;

namespace Hardware.Application.DTOs.Inventory;

public sealed record CreateProductDto
{
    public string SKU { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Barcode { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? SupplierId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal CostPrice { get; init; }
    public UnitOfMeasure Unit { get; init; } = UnitOfMeasure.Piece;
    public int ReorderLevel { get; init; }
    public int ReorderQuantity { get; init; }
}

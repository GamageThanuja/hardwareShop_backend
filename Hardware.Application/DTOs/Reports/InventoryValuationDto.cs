namespace Hardware.Application.DTOs.Reports;

public sealed record InventoryValuationDto
{
    public int TotalProducts { get; init; }
    public int TotalUnits { get; init; }
    public decimal TotalValue { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IReadOnlyList<InventoryValuationItemDto> Items { get; init; } = [];
}

public sealed record InventoryValuationItemDto(
    Guid ProductId,
    string SKU,
    string ProductName,
    string CategoryName,
    int TotalUnits,
    decimal CostPrice,
    decimal TotalValue);

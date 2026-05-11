namespace Hardware.Application.DTOs.Inventory;

public sealed record TransferStockDto(
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string? Notes);

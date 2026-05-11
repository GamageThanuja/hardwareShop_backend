namespace Hardware.Application.DTOs.Purchasing;

public sealed record CreatePurchaseReturnDto(
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime? ReturnDate,
    string? Reason,
    string? Notes,
    IReadOnlyList<CreatePurchaseReturnItemDto> Items);

public sealed record CreatePurchaseReturnItemDto(
    Guid ProductId,
    int Quantity,
    string? Notes);

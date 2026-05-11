namespace Hardware.Application.DTOs.Sales;

public sealed record CreateSalesReturnDto(
    Guid SalesOrderId,
    Guid WarehouseId,
    DateTime? ReturnDate,
    string? Reason,
    string? Notes,
    IReadOnlyList<CreateSalesReturnItemDto> Items);

public sealed record CreateSalesReturnItemDto(
    Guid ProductId,
    int Quantity,
    string? Notes);

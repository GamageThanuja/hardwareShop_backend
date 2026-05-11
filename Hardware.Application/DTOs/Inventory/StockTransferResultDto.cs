namespace Hardware.Application.DTOs.Inventory;

public sealed record StockTransferResultDto(
    InventoryTransactionDto TransferOut,
    InventoryTransactionDto TransferIn);

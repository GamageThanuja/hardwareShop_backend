using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;

namespace Hardware.Application.Services.Inventory;

public interface IStockService
{
    Task<IReadOnlyList<StockItemDto>> GetStockByProductAsync(Guid productId, CancellationToken ct = default);
    Task<PagedResult<StockItemDto>> GetStockByWarehouseAsync(Guid warehouseId, PagedRequestDto request, CancellationToken ct = default);
    Task<InventoryTransactionDto> RecordTransactionAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default);
    Task<PagedResult<InventoryTransactionDto>> GetTransactionsAsync(PagedRequestDto request, Guid? productId, Guid? warehouseId, CancellationToken ct = default);
    Task<StockTransferResultDto> TransferStockAsync(TransferStockDto dto, CancellationToken ct = default);
}

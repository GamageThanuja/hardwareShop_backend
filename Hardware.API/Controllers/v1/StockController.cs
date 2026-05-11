using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Inventory;
using Hardware.Application.Services.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class StockController(
    IStockService stockService,
    ILogger<StockController> logger) : AppControllerBase(logger)
{
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
    {
        var result = await stockService.GetStockByProductAsync(productId, ct);
        return Ok(ApiResponse<IReadOnlyList<StockItemDto>>.Success(result));
    }

    [HttpGet("warehouse/{warehouseId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId, [FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await stockService.GetStockByWarehouseAsync(warehouseId, request, ct);
        return Ok(ApiResponse<PagedResult<StockItemDto>>.Success(result));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] PagedRequestDto request,
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        CancellationToken ct)
    {
        var result = await stockService.GetTransactionsAsync(request, productId, warehouseId, ct);
        return Ok(ApiResponse<PagedResult<InventoryTransactionDto>>.Success(result));
    }

    [HttpPost("transactions")]
    [Authorize(Policy = "RequireStoreKeeper")]
    [ProducesResponseType(typeof(ApiResponse<InventoryTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordTransaction([FromBody] CreateInventoryTransactionDto dto, CancellationToken ct)
    {
        var result = await stockService.RecordTransactionAsync(dto, ct);
        return CreatedAtAction(nameof(GetTransactions), ApiResponse<InventoryTransactionDto>.Success(result, "Transaction recorded."));
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "RequireStoreKeeper")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer([FromBody] TransferStockDto dto, CancellationToken ct)
    {
        var result = await stockService.TransferStockAsync(dto, ct);
        return Ok(ApiResponse<StockTransferResultDto>.Success(result, "Stock transferred."));
    }
}

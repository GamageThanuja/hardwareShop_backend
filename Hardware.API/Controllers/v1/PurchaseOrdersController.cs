using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Purchasing;
using Hardware.Application.Services.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class PurchaseOrdersController(
    IPurchaseOrderService purchaseOrderService,
    ILogger<PurchaseOrdersController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await purchaseOrderService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await purchaseOrderService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await purchaseOrderService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PurchaseOrderDto>.Success(result, "Purchase order created."));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePurchaseOrderStatusDto dto, CancellationToken ct)
    {
        var result = await purchaseOrderService.UpdateStatusAsync(id, dto.Status, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Success(result, "Purchase order status updated."));
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "RequireStoreKeeper")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveItems(Guid id, [FromBody] IReadOnlyList<ReceivePurchaseOrderItemDto> items, CancellationToken ct)
    {
        var result = await purchaseOrderService.ReceiveItemsAsync(id, items, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Success(result, "Items received."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await purchaseOrderService.DeleteAsync(id, ct);
        return NoContent();
    }
}

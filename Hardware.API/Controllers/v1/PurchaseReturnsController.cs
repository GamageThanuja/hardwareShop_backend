using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Purchasing;
using Hardware.Application.Services.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/purchase-returns")]
[Authorize]
public sealed class PurchaseReturnsController(
    IPurchaseReturnService purchaseReturnService,
    ILogger<PurchaseReturnsController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseReturnDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] Guid? purchaseOrderId, CancellationToken ct)
    {
        var result = await purchaseReturnService.GetAllAsync(request, purchaseOrderId, ct);
        return Ok(ApiResponse<PagedResult<PurchaseReturnDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await purchaseReturnService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PurchaseReturnDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireStoreKeeper")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnDto dto, CancellationToken ct)
    {
        var result = await purchaseReturnService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PurchaseReturnDto>.Success(result, "Return recorded."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await purchaseReturnService.CancelAsync(id, ct);
        return Ok(ApiResponse<PurchaseReturnDto>.Success(result, "Return cancelled."));
    }
}

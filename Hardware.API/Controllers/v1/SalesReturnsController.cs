using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/sales-returns")]
[Authorize]
public sealed class SalesReturnsController(
    ISalesReturnService salesReturnService,
    ILogger<SalesReturnsController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalesReturnDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, [FromQuery] Guid? salesOrderId, CancellationToken ct)
    {
        var result = await salesReturnService.GetAllAsync(request, salesOrderId, ct);
        return Ok(ApiResponse<PagedResult<SalesReturnDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await salesReturnService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<SalesReturnDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnDto dto, CancellationToken ct)
    {
        var result = await salesReturnService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SalesReturnDto>.Success(result, "Return recorded."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await salesReturnService.CancelAsync(id, ct);
        return Ok(ApiResponse<SalesReturnDto>.Success(result, "Return cancelled."));
    }
}

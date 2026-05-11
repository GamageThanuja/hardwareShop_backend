using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Services.Sales;
using Hardware.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class SalesOrdersController(
    ISalesOrderService salesOrderService,
    ILogger<SalesOrdersController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await salesOrderService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<SalesOrderDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await salesOrderService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<SalesOrderDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var result = await salesOrderService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SalesOrderDto>.Success(result, "Sales order created."));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSalesOrderStatusDto dto, CancellationToken ct)
    {
        var result = await salesOrderService.UpdateStatusAsync(id, dto.Status, ct);
        return Ok(ApiResponse<SalesOrderDto>.Success(result, "Order status updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await salesOrderService.DeleteAsync(id, ct);
        return NoContent();
    }
}

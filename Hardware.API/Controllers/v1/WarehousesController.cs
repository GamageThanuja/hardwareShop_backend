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
public sealed class WarehousesController(
    IWarehouseService warehouseService,
    ILogger<WarehousesController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WarehouseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await warehouseService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<WarehouseDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await warehouseService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<WarehouseDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        var result = await warehouseService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WarehouseDto>.Success(result, "Warehouse created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        var result = await warehouseService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<WarehouseDto>.Success(result, "Warehouse updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await warehouseService.DeleteAsync(id, ct);
        return NoContent();
    }
}

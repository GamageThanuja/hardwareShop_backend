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
public sealed class SuppliersController(
    ISupplierService supplierService,
    ILogger<SuppliersController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await supplierService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await supplierService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<SupplierDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var result = await supplierService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SupplierDto>.Success(result, "Supplier created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        var result = await supplierService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<SupplierDto>.Success(result, "Supplier updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await supplierService.DeleteAsync(id, ct);
        return NoContent();
    }
}

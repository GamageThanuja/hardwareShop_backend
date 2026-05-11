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
public sealed class CategoriesController(
    ICategoryService categoryService,
    ILogger<CategoriesController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await categoryService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<CategoryDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await categoryService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CategoryDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await categoryService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CategoryDto>.Success(result, "Category created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await categoryService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<CategoryDto>.Success(result, "Category updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await categoryService.DeleteAsync(id, ct);
        return NoContent();
    }
}

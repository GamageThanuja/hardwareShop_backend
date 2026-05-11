using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Sales;
using Hardware.Application.Services.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CustomersController(
    ICustomerService customerService,
    ILogger<CustomersController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await customerService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<CustomerDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await customerService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<CustomerDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var result = await customerService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CustomerDto>.Success(result, "Customer created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var result = await customerService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<CustomerDto>.Success(result, "Customer updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await customerService.DeleteAsync(id, ct);
        return NoContent();
    }
}

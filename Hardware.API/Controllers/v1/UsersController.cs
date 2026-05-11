using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Users;
using Hardware.Application.Services.Identity;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "RequireAdmin")]
public sealed class UsersController(
    IUserService userService,
    ILogger<UsersController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await userService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<UserDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await userService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<UserDto>.Success(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var result = await userService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UserDto>.Success(result, "User created."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var result = await userService.UpdateAsync(id, dto, ct);
        return Ok(ApiResponse<UserDto>.Success(result, "User updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await userService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesDto dto, CancellationToken ct)
    {
        var result = await userService.AssignRolesAsync(id, dto, ct);
        return Ok(ApiResponse<UserDto>.Success(result, "Roles updated."));
    }
}

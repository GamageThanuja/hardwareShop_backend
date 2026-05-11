using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Users;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/me")]
[Authorize]
public sealed class MeController(
    IUserService userService,
    ILogger<MeController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = GetUserId() ?? throw new UnauthorizedException();
        var result = await userService.GetByIdAsync(userId, ct);
        return Ok(ApiResponse<UserDto>.Success(result));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateMyProfileDto dto, CancellationToken ct)
    {
        var userId = GetUserId() ?? throw new UnauthorizedException();
        var result = await userService.UpdateProfileAsync(userId, dto, ct);
        return Ok(ApiResponse<UserDto>.Success(result, "Profile updated."));
    }

    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = GetUserId() ?? throw new UnauthorizedException();
        await userService.ChangePasswordAsync(userId, dto, ct);
        return NoContent();
    }
}

using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Auth;
using Hardware.Application.Services.Authentication;
using Hardware.Shared.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthController> logger) : AppControllerBase(logger)
{
    private const string RefreshCookieName = "rt";
    private const string RefreshCookiePath = "/api/v1/auth";
    private readonly JwtSettings _jwt = jwtOptions.Value;

    [HttpPost("register")]
    [Authorize(Policy = "RequireAdmin")]
    [EnableRateLimiting("auth_endpoints")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(result);

        SetRefreshCookie(result.Data!.RefreshToken);
        return Created($"/api/v1/users/{result.Data.UserId}", result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_endpoints")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await authService.LoginAsync(dto, ct);
        if (!result.IsSuccess) return Unauthorized(result);

        SetRefreshCookie(result.Data!.RefreshToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth_endpoints")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto? body, CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName] ?? body?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(ApiResponse.Failure("Refresh token required."));

        var result = await authService.RefreshAsync(new RefreshTokenDto(refreshToken), ct);
        if (!result.IsSuccess)
        {
            DeleteRefreshCookie();
            return Unauthorized(result);
        }

        SetRefreshCookie(result.Data!.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
            await authService.LogoutAsync(refreshToken, ct);

        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await authService.LogoutAllAsync(userId.Value, ct);
        DeleteRefreshCookie();
        return NoContent();
    }

    private void SetRefreshCookie(string token) =>
        Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays),
            Path = RefreshCookiePath,
            IsEssential = true
        });

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = RefreshCookiePath });
}

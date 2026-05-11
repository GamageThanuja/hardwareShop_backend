using System.Security.Claims;
using Hardware.API.Common;
using Hardware.Application.DTOs.Auth;
using Hardware.Application.Exceptions;
using Hardware.Application.Services.Authentication;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/me/sessions")]
[Authorize]
public sealed class MeSessionsController(
    IAuthService authService,
    ILogger<MeSessionsController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SessionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = GetUserId() ?? throw new UnauthorizedException();
        var currentSid = User.FindFirstValue(CustomClaimTypes.SessionId);
        var sessions = await authService.ListSessionsAsync(userId, currentSid, ct);
        return Ok(sessions);
    }

    [HttpDelete("{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(string sessionId, CancellationToken ct)
    {
        var userId = GetUserId() ?? throw new UnauthorizedException();
        await authService.RevokeSessionAsync(userId, sessionId, ct);
        return NoContent();
    }
}

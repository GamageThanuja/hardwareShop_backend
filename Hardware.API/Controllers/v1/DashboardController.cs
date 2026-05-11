using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Dashboard;
using Hardware.Application.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class DashboardController(
    IDashboardService dashboardService,
    ILogger<DashboardController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await dashboardService.GetDashboardAsync(ct);
        return Ok(ApiResponse<DashboardDto>.Success(result));
    }
}

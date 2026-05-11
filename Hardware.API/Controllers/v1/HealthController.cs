using Hardware.API.Common;
using Hardware.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public sealed class HealthController(ILogger<HealthController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(ApiResponse<object>.Success(new
        {
            status = "healthy",
            application = "Hardware",
            timestampUtc = DateTime.UtcNow
        }));
}

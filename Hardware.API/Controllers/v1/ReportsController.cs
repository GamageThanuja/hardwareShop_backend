using Hardware.API.Common;
using Hardware.Application.Common;
using Hardware.Application.DTOs.Common;
using Hardware.Application.DTOs.Reports;
using Hardware.Application.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "RequireManager")]
public sealed class ReportsController(
    IReportService reportService,
    ILogger<ReportsController> logger) : AppControllerBase(logger)
{
    [HttpGet("sales")]
    [ProducesResponseType(typeof(ApiResponse<SalesReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var from = dateFrom ?? DateTime.UtcNow.AddMonths(-1);
        var to   = dateTo   ?? DateTime.UtcNow;
        var result = await reportService.GetSalesReportAsync(from, to, ct);
        return Ok(ApiResponse<SalesReportDto>.Success(result));
    }

    [HttpGet("inventory")]
    [ProducesResponseType(typeof(ApiResponse<InventoryValuationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryValuation([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await reportService.GetInventoryValuationAsync(request, ct);
        return Ok(ApiResponse<InventoryValuationDto>.Success(result));
    }

    [HttpGet("purchases")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchaseReport(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var from = dateFrom ?? DateTime.UtcNow.AddMonths(-1);
        var to   = dateTo   ?? DateTime.UtcNow;
        var result = await reportService.GetPurchaseReportAsync(from, to, ct);
        return Ok(ApiResponse<PurchaseReportDto>.Success(result));
    }
}

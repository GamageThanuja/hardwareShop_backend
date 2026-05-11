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
public sealed class PaymentsController(
    IPaymentService paymentService,
    ILogger<PaymentsController> logger) : AppControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? salesOrderId, [FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var result = await paymentService.GetAllAsync(salesOrderId, request, ct);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await paymentService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PaymentDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireCashier")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Record([FromBody] RecordPaymentDto dto, CancellationToken ct)
    {
        var result = await paymentService.RecordAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentDto>.Success(result, "Payment recorded."));
    }

    [HttpPost("{id:guid}/void")]
    [Authorize(Policy = "RequireManager")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPaymentDto dto, CancellationToken ct)
    {
        var result = await paymentService.VoidAsync(id, dto, ct);
        return Ok(ApiResponse<PaymentDto>.Success(result, "Payment voided."));
    }
}

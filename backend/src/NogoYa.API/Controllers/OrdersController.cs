using Microsoft.AspNetCore.Mvc;
using NogoYa.API.Extensions;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces.Services;

namespace NogoYa.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    public OrdersController(IOrderService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => (await _service.GetByIdAsync(id, ct)).ToActionResult();

    [HttpGet("by-store/{storeId:guid}")]
    public async Task<IActionResult> GetByStore(Guid storeId, CancellationToken ct)
        => (await _service.GetByStoreAsync(storeId, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
        => (await _service.UpdateStatusAsync(id, dto, ct)).ToActionResult();

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string? reason, CancellationToken ct)
        => (await _service.CancelAsync(id, reason, ct)).ToActionResult();
}

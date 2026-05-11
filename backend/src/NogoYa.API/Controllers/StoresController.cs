using Microsoft.AspNetCore.Mvc;
using NogoYa.API.Extensions;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces.Services;

namespace NogoYa.API.Controllers;

[ApiController]
[Route("api/v1/stores")]
[Produces("application/json")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _service;
    public StoresController(IStoreService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => (await _service.GetAllAsync(ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => (await _service.GetByIdAsync(id, ct)).ToActionResult();

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        => (await _service.GetBySlugAsync(slug, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto, CancellationToken ct)
        => (await _service.UpdateAsync(id, dto, ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _service.DeleteAsync(id, ct)).ToActionResult();
}

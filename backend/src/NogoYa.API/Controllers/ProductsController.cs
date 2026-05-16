using Microsoft.AspNetCore.Mvc;
using NogoYa.API.Extensions;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Services;

namespace NogoYa.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes =
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel",
        "application/octet-stream"
    };

    private readonly IProductService _service;
    private readonly IProductImportService _importService;
    private readonly IExcelProductReader _excelReader;

    public ProductsController(
        IProductService service,
        IProductImportService importService,
        IExcelProductReader excelReader)
    {
        _service = service;
        _importService = importService;
        _excelReader = excelReader;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ProductFilterDto filter, CancellationToken ct)
        => (await _service.SearchAsync(filter, ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => (await _service.GetByIdAsync(id, ct)).ToActionResult();

    [HttpGet("by-store/{storeId:guid}")]
    public async Task<IActionResult> GetByStore(Guid storeId, CancellationToken ct)
        => (await _service.GetByStoreAsync(storeId, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        => (await _service.UpdateAsync(id, dto, ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _service.DeleteAsync(id, ct)).ToActionResult();

    /// <summary>
    /// Returns the official xlsx template (with headers + sample rows + a
    /// help sheet) for bulk import. The user downloads it, fills it in and
    /// uploads back via POST /import.
    /// </summary>
    [HttpGet("import/template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public IActionResult DownloadImportTemplate()
    {
        var bytes = _excelReader.BuildImportTemplate();
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "nogoya-import-products-template.xlsx");
    }

    [HttpPost("import")]
    [RequestSizeLimit(MaxUploadBytes)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(
        [FromForm] Guid storeId,
        [FromForm] IFormFile file,
        [FromForm] ImportMode mode = ImportMode.Upsert,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Debe adjuntar un archivo .xlsx no vacío.", code = "EMPTY_FILE" });
        if (file.Length > MaxUploadBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { error = "El archivo excede el tamaño máximo permitido (10 MB).", code = "FILE_TOO_LARGE" });
        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)
            && !file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Formato no soportado. Use .xlsx.", code = "INVALID_FILE_TYPE" });

        await using var stream = file.OpenReadStream();
        var result = await _importService.ImportAsync(storeId, stream, mode, ct);
        return result.ToActionResult();
    }
}

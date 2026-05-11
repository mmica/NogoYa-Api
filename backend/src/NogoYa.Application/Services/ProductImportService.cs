using Microsoft.Extensions.Logging;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Services;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Services;

public class ProductImportService : IProductImportService
{
    private readonly IUnitOfWork _uow;
    private readonly IExcelProductReader _reader;
    private readonly ILogger<ProductImportService> _logger;

    public ProductImportService(IUnitOfWork uow, IExcelProductReader reader, ILogger<ProductImportService> logger)
    { _uow = uow; _reader = reader; _logger = logger; }

    public async Task<Result<ProductImportResultDto>> ImportAsync(
        Guid storeId, Stream xlsxStream, ImportMode mode, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetByIdAsync(storeId, ct);
        if (store is null)
            return Result.Failure<ProductImportResultDto>($"Comercio '{storeId}' no encontrado.", "STORE_NOT_FOUND");

        var errors = new List<ProductImportErrorDto>();
        var toInsert = new List<Product>();
        var toUpdate = new List<Product>();
        int totalRows = 0, skipped = 0;

        var existing = (await _uow.Products.GetByStoreAsync(storeId, ct))
            .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
            .ToDictionary(p => p.Sku!, StringComparer.OrdinalIgnoreCase);

        await foreach (var row in _reader.ReadAsync(xlsxStream, ct))
        {
            totalRows++;
            if (string.IsNullOrWhiteSpace(row.Name))
            { errors.Add(new(row.RowNumber, row.Sku, "El nombre es obligatorio.")); continue; }
            if (!row.Price.HasValue || row.Price < 0)
            { errors.Add(new(row.RowNumber, row.Sku, "Precio inválido (>= 0).")); continue; }
            if (row.DiscountPercent is < 0 or > 100)
            { errors.Add(new(row.RowNumber, row.Sku, "Descuento fuera de rango (0-100).")); continue; }
            if (row.Stock is < 0)
            { errors.Add(new(row.RowNumber, row.Sku, "Stock no puede ser negativo.")); continue; }

            var sku = row.Sku?.Trim();
            if (!string.IsNullOrWhiteSpace(sku) && existing.TryGetValue(sku!, out var match))
            {
                if (mode == ImportMode.InsertOnly) { skipped++; continue; }
                match.Name = row.Name!.Trim();
                match.Description = row.Description?.Trim();
                match.ImageUrl = row.ImageUrl?.Trim();
                match.Price = row.Price!.Value;
                match.DiscountPercent = row.DiscountPercent ?? 0m;
                match.Stock = row.Stock ?? 0;
                match.IsAvailable = row.IsAvailable ?? true;
                toUpdate.Add(match);
            }
            else
            {
                toInsert.Add(new Product
                {
                    StoreId = storeId, Sku = sku, Name = row.Name!.Trim(),
                    Description = row.Description?.Trim(), ImageUrl = row.ImageUrl?.Trim(),
                    Price = row.Price!.Value, DiscountPercent = row.DiscountPercent ?? 0m,
                    Stock = row.Stock ?? 0, IsAvailable = row.IsAvailable ?? true
                });
            }
        }

        if (toInsert.Count == 0 && toUpdate.Count == 0)
            return Result.Success(new ProductImportResultDto(totalRows, 0, 0, skipped, errors.Count, errors));

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var p in toInsert) await _uow.Products.AddAsync(p, ct);
            foreach (var p in toUpdate) _uow.Products.Update(p);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Import OK. Store={Store} Ins={Ins} Upd={Upd} Err={Err}",
                storeId, toInsert.Count, toUpdate.Count, errors.Count);

            return Result.Success(new ProductImportResultDto(
                totalRows, toInsert.Count, toUpdate.Count, skipped, errors.Count, errors));
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Import failed for store {Store}", storeId);
            return Result.Failure<ProductImportResultDto>(
                "No se pudo completar la importación.", "IMPORT_FAILED");
        }
    }
}

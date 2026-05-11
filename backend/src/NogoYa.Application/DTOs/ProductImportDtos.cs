namespace NogoYa.Application.DTOs;

public record ProductImportRow(
    int RowNumber, string? Sku, string? Name, string? Description,
    decimal? Price, decimal? DiscountPercent, int? Stock,
    string? ImageUrl, bool? IsAvailable);

public record ProductImportErrorDto(int RowNumber, string? Sku, string Message);

public record ProductImportResultDto(
    int TotalRows, int Imported, int Updated, int Skipped, int Failed,
    IReadOnlyList<ProductImportErrorDto> Errors);

public enum ImportMode
{
    InsertOnly = 0,
    Upsert = 1
}

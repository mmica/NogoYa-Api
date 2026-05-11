using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces;

public interface IExcelProductReader
{
    IAsyncEnumerable<ProductImportRow> ReadAsync(Stream xlsxStream, CancellationToken ct = default);
}

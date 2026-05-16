using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces;

public interface IExcelProductReader
{
    IAsyncEnumerable<ProductImportRow> ReadAsync(Stream xlsxStream, CancellationToken ct = default);

    /// <summary>
    /// Build a ready-to-use .xlsx template with the expected headers and a few
    /// example rows. Users download this from the UI so they never guess column
    /// names.
    /// </summary>
    byte[] BuildImportTemplate();
}

using System.Globalization;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces;

namespace NogoYa.Infrastructure.Services;

public class ExcelProductReader : IExcelProductReader
{
    private static readonly string[] HeaderAliases_Sku = { "sku", "codigo", "código", "code" };
    private static readonly string[] HeaderAliases_Name = { "name", "nombre", "producto", "product" };
    private static readonly string[] HeaderAliases_Description = { "description", "descripcion", "descripción", "detalle" };
    private static readonly string[] HeaderAliases_Price = { "price", "precio" };
    private static readonly string[] HeaderAliases_Discount = { "discountpercent", "discount", "descuento", "%desc" };
    private static readonly string[] HeaderAliases_Stock = { "stock", "cantidad", "qty" };
    private static readonly string[] HeaderAliases_Image = { "imageurl", "image", "imagen", "foto" };
    private static readonly string[] HeaderAliases_Available = { "isavailable", "available", "disponible", "activo" };

    public async IAsyncEnumerable<ProductImportRow> ReadAsync(
        Stream xlsxStream, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook(xlsxStream);
        var sheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException("El archivo no contiene hojas.");
        var headerRow = sheet.FirstRowUsed()
            ?? throw new InvalidDataException("La hoja está vacía.");
        var map = BuildHeaderMap(headerRow);

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            ct.ThrowIfCancellationRequested();
            if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetString()))) continue;

            yield return new ProductImportRow(
                RowNumber: row.RowNumber(),
                Sku: TryReadString(row, map, "sku"),
                Name: TryReadString(row, map, "name"),
                Description: TryReadString(row, map, "description"),
                Price: TryReadDecimal(row, map, "price"),
                DiscountPercent: TryReadDecimal(row, map, "discount"),
                Stock: TryReadInt(row, map, "stock"),
                ImageUrl: TryReadString(row, map, "image"),
                IsAvailable: TryReadBool(row, map, "available")
            );
            await Task.Yield();
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLRow headerRow)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var normalized = Normalize(cell.GetString());
            var logical = MatchLogical(normalized);
            if (logical is not null && !dict.ContainsKey(logical))
                dict[logical] = cell.Address.ColumnNumber;
        }
        return dict;
    }

    private static string? MatchLogical(string header)
    {
        if (HeaderAliases_Sku.Contains(header)) return "sku";
        if (HeaderAliases_Name.Contains(header)) return "name";
        if (HeaderAliases_Description.Contains(header)) return "description";
        if (HeaderAliases_Price.Contains(header)) return "price";
        if (HeaderAliases_Discount.Contains(header)) return "discount";
        if (HeaderAliases_Stock.Contains(header)) return "stock";
        if (HeaderAliases_Image.Contains(header)) return "image";
        if (HeaderAliases_Available.Contains(header)) return "available";
        return null;
    }

    private static string Normalize(string raw)
        => new string(raw.Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());

    private static string? TryReadString(IXLRow row, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var col)) return null;
        var value = row.Cell(col).GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal? TryReadDecimal(IXLRow row, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var col)) return null;
        var cell = row.Cell(col);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue<decimal>(out var dec)) return dec;
        if (decimal.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) return p;
        return null;
    }

    private static int? TryReadInt(IXLRow row, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var col)) return null;
        var cell = row.Cell(col);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue<int>(out var i)) return i;
        if (int.TryParse(cell.GetString(), out var p)) return p;
        return null;
    }

    private static bool? TryReadBool(IXLRow row, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var col)) return null;
        var raw = row.Cell(col).GetString().Trim().ToLowerInvariant();
        return raw switch
        {
            "" => null,
            "1" or "true" or "si" or "sí" or "yes" or "y" or "x" => true,
            "0" or "false" or "no" or "n" => false,
            _ => null
        };
    }
}

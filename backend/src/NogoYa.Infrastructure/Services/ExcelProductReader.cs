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

    public byte[] BuildImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Productos");

        // --- Header row -------------------------------------------------------
        string[] headers =
        {
            "Sku", "Name", "Description", "Price",
            "DiscountPercent", "Stock", "ImageUrl", "IsAvailable"
        };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(95, 109, 46); // brand-600
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromArgb(74, 85, 38);
        }

        // --- Example rows -----------------------------------------------------
        // Realistic but fictional samples so the user gets a feel for the format.
        (string sku, string name, string desc, decimal price, decimal disc, int stock, string img, bool active)[] examples =
        {
            ("PAN-001", "Pan francés",     "Pan recién horneado",            500m,  0m, 50, "", true),
            ("QUE-001", "Queso cremoso",   "Queso cremoso 250g",            1200m, 10m, 20, "", true),
            ("VIN-001", "Vino tinto",      "Malbec 750ml",                  3500m, 15m,  8, "", true)
        };
        for (int r = 0; r < examples.Length; r++)
        {
            var ex = examples[r];
            var row = r + 2;
            sheet.Cell(row, 1).Value = ex.sku;
            sheet.Cell(row, 2).Value = ex.name;
            sheet.Cell(row, 3).Value = ex.desc;
            sheet.Cell(row, 4).Value = ex.price;
            sheet.Cell(row, 5).Value = ex.disc;
            sheet.Cell(row, 6).Value = ex.stock;
            sheet.Cell(row, 7).Value = ex.img;
            sheet.Cell(row, 8).Value = ex.active;
        }

        // --- Polish -----------------------------------------------------------
        sheet.Column(4).Style.NumberFormat.Format = "#,##0.00";  // Price
        sheet.Column(5).Style.NumberFormat.Format = "0.00";       // Discount %
        sheet.Columns().AdjustToContents();
        sheet.SheetView.FreezeRows(1);

        // Add notes in a second sheet so the user knows how columns are interpreted.
        var help = workbook.AddWorksheet("Instrucciones");
        var lines = new[]
        {
            ("Columna",           "Tipo",          "Descripción"),
            ("Sku",               "texto (opcional)", "Código único dentro del comercio. Sirve para identificar al producto en futuras importaciones."),
            ("Name",              "texto (obligatorio)", "Nombre del producto. Máximo 150 caracteres."),
            ("Description",       "texto (opcional)", "Descripción larga. Máximo 2000 caracteres."),
            ("Price",             "decimal (obligatorio)", "Precio de venta sin descuento. Mayor o igual a 0."),
            ("DiscountPercent",   "decimal (opcional)", "Porcentaje de descuento entre 0 y 100. Vacío o 0 = sin descuento."),
            ("Stock",             "entero (opcional)", "Cantidad disponible. Mayor o igual a 0. Vacío = 0."),
            ("ImageUrl",          "texto (opcional)", "URL pública de la imagen del producto."),
            ("IsAvailable",       "booleano (opcional)", "Acepta 'true/false', 'si/no', '1/0', 'x'. Vacío = true."),
        };
        for (int i = 0; i < lines.Length; i++)
        {
            var (col, type, desc) = lines[i];
            help.Cell(i + 1, 1).Value = col;
            help.Cell(i + 1, 2).Value = type;
            help.Cell(i + 1, 3).Value = desc;
            if (i == 0)
            {
                var range = help.Range(i + 1, 1, i + 1, 3);
                range.Style.Font.Bold = true;
                range.Style.Fill.BackgroundColor = XLColor.FromArgb(231, 236, 209); // brand-100
            }
        }
        help.Columns().AdjustToContents();
        help.Column(3).Width = Math.Min(help.Column(3).Width, 90);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

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

using NogoYa.Application.Common;
using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces.Services;

public interface IProductImportService
{
    Task<Result<ProductImportResultDto>> ImportAsync(
        Guid storeId, Stream xlsxStream, ImportMode mode, CancellationToken ct = default);
}

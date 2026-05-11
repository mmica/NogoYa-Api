using NogoYa.Application.Common;
using NogoYa.Application.DTOs;

namespace NogoYa.Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<PagedResult<ProductDto>>> SearchAsync(ProductFilterDto filter, CancellationToken ct = default);
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProductDto>>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

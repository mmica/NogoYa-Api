using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetWithStoreAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Product>> SearchAsync(ProductFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);
}

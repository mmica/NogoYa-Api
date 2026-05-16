using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Domain.Entities;

namespace NogoYa.Application.Interfaces.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<Store?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default);
    Task<PagedResult<Store>> SearchAsync(StoreFilterDto filter, CancellationToken ct = default);
}

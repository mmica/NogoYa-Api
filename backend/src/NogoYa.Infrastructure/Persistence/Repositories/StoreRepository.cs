using Microsoft.EntityFrameworkCore;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces.Repositories;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Repositories;

public class StoreRepository : Repository<Store>, IStoreRepository
{
    public StoreRepository(NogoYaDbContext ctx) : base(ctx) { }

    public Task<Store?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => Set.Include(s => s.Products).FirstOrDefaultAsync(s => s.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
        => Set.AnyAsync(s => s.Slug == slug && (excludeId == null || s.Id != excludeId), ct);

    public async Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().Include(s => s.Products)
            .Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);

    public async Task<PagedResult<Store>> SearchAsync(StoreFilterDto filter, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(s => s.Products).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, term)
                || EF.Functions.ILike(s.Slug, term)
                || (s.Description != null && EF.Functions.ILike(s.Description, term)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Store>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = total
        };
    }
}

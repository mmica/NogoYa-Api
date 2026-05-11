using Microsoft.EntityFrameworkCore;
using NogoYa.Application.Common;
using NogoYa.Application.DTOs;
using NogoYa.Application.Interfaces.Repositories;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(NogoYaDbContext ctx) : base(ctx) { }

    public Task<Product?> GetWithStoreAsync(Guid id, CancellationToken ct = default)
        => Set.Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(p => p.Store)
            .Where(p => p.StoreId == storeId && p.IsAvailable)
            .OrderBy(p => p.Name).ToListAsync(ct);

    public async Task<PagedResult<Product>> SearchAsync(ProductFilterDto filter, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(p => p.Store).AsQueryable();
        if (filter.StoreId.HasValue) query = query.Where(p => p.StoreId == filter.StoreId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term)
                || (p.Description != null && EF.Functions.ILike(p.Description, term)));
        }
        if (filter.MinPrice.HasValue) query = query.Where(p => p.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        if (filter.OnSale == true) query = query.Where(p => p.DiscountPercent > 0);
        query = query.Where(p => p.IsAvailable);

        var total = await query.CountAsync(ct);
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var size = filter.PageSize is <= 0 or > 100 ? 20 : filter.PageSize;
        var items = await query.OrderBy(p => p.Name)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return new PagedResult<Product> { Items = items, Page = page, PageSize = size, TotalItems = total };
    }
}

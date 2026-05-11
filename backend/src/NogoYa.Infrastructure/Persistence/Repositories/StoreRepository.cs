using Microsoft.EntityFrameworkCore;
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
}

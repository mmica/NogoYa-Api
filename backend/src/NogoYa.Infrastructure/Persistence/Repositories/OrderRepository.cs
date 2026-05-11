using Microsoft.EntityFrameworkCore;
using NogoYa.Application.Interfaces.Repositories;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(NogoYaDbContext ctx) : base(ctx) { }

    public Task<Order?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
        => Set.Include(o => o.Store).Include(o => o.Items)
              .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
        => await Set.AsNoTracking().Include(o => o.Store).Include(o => o.Items)
            .Where(o => o.StoreId == storeId)
            .OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"NGY-{year}-";
        var count = await Set.IgnoreQueryFilters()
            .CountAsync(o => o.OrderNumber.StartsWith(prefix), ct);
        return $"{prefix}{(count + 1):D6}";
    }
}

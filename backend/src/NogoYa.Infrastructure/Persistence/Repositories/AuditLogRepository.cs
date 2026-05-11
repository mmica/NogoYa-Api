using Microsoft.EntityFrameworkCore;
using NogoYa.Application.Interfaces.Repositories;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(NogoYaDbContext ctx) : base(ctx) { }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
}

using NogoYa.Application.Interfaces.Repositories;

namespace NogoYa.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IStoreRepository Stores { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

using Microsoft.EntityFrameworkCore.Storage;
using NogoYa.Application.Interfaces;
using NogoYa.Application.Interfaces.Repositories;
using NogoYa.Infrastructure.Persistence.Repositories;

namespace NogoYa.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly NogoYaDbContext _ctx;
    private IDbContextTransaction? _transaction;

    public IStoreRepository Stores { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public IAuditLogRepository AuditLogs { get; }

    public UnitOfWork(NogoYaDbContext ctx)
    {
        _ctx = ctx;
        Stores = new StoreRepository(ctx);
        Products = new ProductRepository(ctx);
        Orders = new OrderRepository(ctx);
        AuditLogs = new AuditLogRepository(ctx);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) return;
        _transaction = await _ctx.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        try { await _transaction.CommitAsync(ct); }
        finally { await _transaction.DisposeAsync(); _transaction = null; }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        try { await _transaction.RollbackAsync(ct); }
        finally { await _transaction.DisposeAsync(); _transaction = null; }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        await _ctx.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

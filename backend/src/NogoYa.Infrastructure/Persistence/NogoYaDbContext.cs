using Microsoft.EntityFrameworkCore;
using NogoYa.Domain.Common;
using NogoYa.Domain.Entities;

namespace NogoYa.Infrastructure.Persistence;

public class NogoYaDbContext : DbContext
{
    public NogoYaDbContext(DbContextOptions<NogoYaDbContext> options) : base(options) { }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NogoYaDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(NogoYaDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }
        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder builder) where T : BaseEntity
        => builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
}

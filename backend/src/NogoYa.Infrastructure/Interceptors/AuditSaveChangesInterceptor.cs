using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NogoYa.Application.Common;
using NogoYa.Domain.Common;
using NogoYa.Domain.Entities;
using NogoYa.Domain.Enums;

namespace NogoYa.Infrastructure.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser) => _currentUser = currentUser;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null) ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ApplyAudit(DbContext ctx)
    {
        var now = DateTime.UtcNow;
        var user = _currentUser.UserName ?? _currentUser.UserId ?? "system";
        var logsToAdd = new List<AuditLog>();

        foreach (var entry in ctx.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    logsToAdd.Add(BuildLog(entry, AuditAction.Create, user));
                    break;

                case EntityState.Modified:
                    if (entry.Entity.IsDeleted && entry.OriginalValues[nameof(BaseEntity.IsDeleted)] as bool? == false)
                    {
                        entry.Entity.DeletedAt = now;
                        logsToAdd.Add(BuildLog(entry, AuditAction.Delete, user));
                    }
                    else
                    {
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = user;
                        logsToAdd.Add(BuildLog(entry, DetectAction(entry), user));
                    }
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    logsToAdd.Add(BuildLog(entry, AuditAction.Delete, user));
                    break;
            }
        }

        if (logsToAdd.Count > 0) ctx.Set<AuditLog>().AddRange(logsToAdd);
    }

    private AuditAction DetectAction(EntityEntry<BaseEntity> entry)
    {
        if (entry.Entity is Product)
        {
            if (PropertyChanged(entry, nameof(Product.Price)) ||
                PropertyChanged(entry, nameof(Product.DiscountPercent))) return AuditAction.PriceChange;
            if (PropertyChanged(entry, nameof(Product.Stock))) return AuditAction.StockChange;
        }
        if (entry.Entity is Order && PropertyChanged(entry, nameof(Order.Status))) return AuditAction.StatusChange;
        return AuditAction.Update;
    }

    private static bool PropertyChanged(EntityEntry entry, string name)
    {
        var prop = entry.Property(name);
        return !Equals(prop.OriginalValue, prop.CurrentValue);
    }

    private AuditLog BuildLog(EntityEntry<BaseEntity> entry, AuditAction action, string user)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changed = new List<string>();

        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey()) continue;
            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                    break;
                case EntityState.Deleted:
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    break;
                case EntityState.Modified when !Equals(prop.OriginalValue, prop.CurrentValue):
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                    changed.Add(prop.Metadata.Name);
                    break;
            }
        }

        return new AuditLog
        {
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id,
            Action = action,
            OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues, JsonOpts) : null,
            NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues, JsonOpts) : null,
            ChangedColumns = changed.Count > 0 ? string.Join(",", changed) : null,
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName ?? user,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent
        };
    }
}

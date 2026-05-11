using NogoYa.Domain.Enums;

namespace NogoYa.Application.DTOs;

public record AuditLogDto(
    Guid Id, string EntityName, Guid EntityId, AuditAction Action,
    string? OldValues, string? NewValues, string? ChangedColumns,
    string? UserId, string? UserName, string? Reason, DateTime CreatedAt);

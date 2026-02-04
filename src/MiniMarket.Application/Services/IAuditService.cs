using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface IAuditService
{
    Task<Result> LogAsync(
        string entityType,
        Guid entityId,
        string action,
        object? oldValues,
        object? newValues,
        Guid? userId,
        string? userName,
        string? ipAddress = null,
        string? userAgent = null);

    Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(string entityType, Guid entityId);
    Task<Result<IReadOnlyList<AuditLogDto>>> GetByUserAsync(Guid userId, int count = 50);
    Task<Result<IReadOnlyList<AuditLogDto>>> GetRecentAsync(int count = 100);
    Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(int pageNumber, int pageSize, string? entityType = null);
    Task<Result> CleanupOldLogsAsync(int daysToKeep = 90);
}

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    Guid? UserId,
    string? UserName,
    string? IpAddress,
    DateTime CreatedAt);

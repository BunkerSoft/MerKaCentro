using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, int count = 50);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 100);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task DeleteOlderThanAsync(DateTime date);
}

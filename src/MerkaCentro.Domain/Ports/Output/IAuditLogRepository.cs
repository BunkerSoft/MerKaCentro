using MerkaCentro.Domain.Entities;

namespace MerkaCentro.Domain.Ports.Output;

public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, int count = 50);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 100);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task DeleteOlderThanAsync(DateTime date);
}

using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await _dbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, int count = 50)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await _dbSet
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteOlderThanAsync(DateTime date)
    {
        var oldLogs = await _dbSet
            .Where(a => a.CreatedAt < date)
            .ToListAsync();

        _dbSet.RemoveRange(oldLogs);
    }
}

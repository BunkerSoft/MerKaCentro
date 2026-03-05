using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class AuditLogRepository : RepositoryBase<AuditLog, Guid>, IAuditLogRepository
{
    public AuditLogRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await DbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, int count = 50)
    {
        return await DbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await DbSet
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await DbSet
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteOlderThanAsync(DateTime date)
    {
        var oldLogs = await DbSet
            .Where(a => a.CreatedAt < date)
            .ToListAsync();

        DbSet.RemoveRange(oldLogs);
    }
}

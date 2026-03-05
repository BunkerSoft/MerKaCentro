using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class SyncQueueRepository : RepositoryBase<SyncQueueItem, Guid>, ISyncQueueRepository
{
    public SyncQueueRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.Status == SyncStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetFailedAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.Status == SyncStatus.Failed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SyncQueueItem?> GetBySyncIdAsync(Guid syncId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.SyncId == syncId, cancellationToken);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(x => x.Status == SyncStatus.Pending, cancellationToken);
    }

    public async Task MarkAllAsFailedAsync(string errorMessage, CancellationToken cancellationToken = default)
    {
        var pending = await GetPendingAsync(1000, cancellationToken);
        foreach (var item in pending)
        {
            item.MarkAsFailed(errorMessage);
        }
    }

    public async Task DeleteSyncedAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        await DbSet
            .Where(x => x.Status == SyncStatus.Synced && x.SyncedAt < olderThan)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

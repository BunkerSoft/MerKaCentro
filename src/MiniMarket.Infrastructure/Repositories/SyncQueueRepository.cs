using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class SyncQueueRepository : ISyncQueueRepository
{
    private readonly MiniMarketDbContext _context;

    public SyncQueueRepository(MiniMarketDbContext context)
    {
        _context = context;
    }

    public async Task<SyncQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SyncQueueItem entity, CancellationToken cancellationToken = default)
    {
        await _context.SyncQueue.AddAsync(entity, cancellationToken);
    }

    public void Update(SyncQueueItem entity)
    {
        _context.SyncQueue.Update(entity);
    }

    public void Remove(SyncQueueItem entity)
    {
        _context.SyncQueue.Remove(entity);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .Where(x => x.Status == SyncStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetFailedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .Where(x => x.Status == SyncStatus.Failed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SyncQueueItem?> GetBySyncIdAsync(Guid syncId, CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
            .FirstOrDefaultAsync(x => x.SyncId == syncId, cancellationToken);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncQueue
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
        await _context.SyncQueue
            .Where(x => x.Status == SyncStatus.Synced && x.SyncedAt < olderThan)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

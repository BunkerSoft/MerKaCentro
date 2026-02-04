using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface ISyncQueueRepository : IRepository<SyncQueueItem, Guid>
{
    Task<IReadOnlyList<SyncQueueItem>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SyncQueueItem>> GetFailedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SyncQueueItem>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<SyncQueueItem?> GetBySyncIdAsync(Guid syncId, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
    Task MarkAllAsFailedAsync(string errorMessage, CancellationToken cancellationToken = default);
    Task DeleteSyncedAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}

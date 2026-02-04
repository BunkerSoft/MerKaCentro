using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface ISyncService
{
    Task<Result> EnqueueAsync<T>(T entity, string operation) where T : class;
    Task<Result<SyncStatusDto>> GetStatusAsync();
    Task<Result<int>> ProcessPendingAsync(int batchSize = 100);
    Task<Result> RetryFailedAsync();
    Task<Result> CleanupSyncedAsync(int daysOld = 7);
}

public record SyncStatusDto(
    int PendingCount,
    int FailedCount,
    bool IsOnline,
    DateTime? LastSyncAt);

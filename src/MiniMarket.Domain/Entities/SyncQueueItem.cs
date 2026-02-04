using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Entities;

public class SyncQueueItem : AggregateRoot<Guid>
{
    public Guid SyncId { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public SyncOperation Operation { get; private set; }
    public string Payload { get; private set; } = default!;
    public SyncStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? LastRetryAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? SyncedAt { get; private set; }

    private SyncQueueItem() : base()
    {
    }

    public static SyncQueueItem Create(
        string entityType,
        Guid entityId,
        SyncOperation operation,
        string payload)
    {
        return new SyncQueueItem
        {
            Id = Guid.NewGuid(),
            SyncId = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            Payload = payload,
            Status = SyncStatus.Pending,
            RetryCount = 0
        };
    }

    public void MarkAsSynced()
    {
        Status = SyncStatus.Synced;
        SyncedAt = DateTime.UtcNow;
        ErrorMessage = null;
        SetUpdated();
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = SyncStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void ResetForRetry()
    {
        Status = SyncStatus.Pending;
        SetUpdated();
    }

    public bool CanRetry(int maxRetries = 5) => RetryCount < maxRetries;

    public TimeSpan GetNextRetryDelay()
    {
        // Exponential backoff: 1s, 2s, 4s, 8s, 16s...
        var seconds = Math.Pow(2, RetryCount);
        return TimeSpan.FromSeconds(Math.Min(seconds, 300)); // Max 5 minutes
    }
}

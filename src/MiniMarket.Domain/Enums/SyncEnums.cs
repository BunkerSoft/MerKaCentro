namespace MiniMarket.Domain.Enums;

public enum SyncOperation
{
    Insert,
    Update,
    Delete
}

public enum SyncStatus
{
    Pending,
    Synced,
    Failed
}

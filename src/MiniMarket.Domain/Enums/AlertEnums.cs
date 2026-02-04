namespace MiniMarket.Domain.Enums;

public enum AlertType
{
    LowStock = 1,
    ExpiringProduct = 2,
    CustomerDebt = 3,
    PendingPurchaseOrder = 4,
    CashRegisterOpen = 5,
    SyncPending = 6
}

public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

public enum AlertStatus
{
    Active = 1,
    Acknowledged = 2,
    Resolved = 3,
    Dismissed = 4
}

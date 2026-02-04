using MiniMarket.Application.Common;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface IAlertService
{
    Task<Result<IReadOnlyList<AlertDto>>> GetActiveAsync();
    Task<Result<IReadOnlyList<AlertDto>>> GetByTypeAsync(AlertType type);
    Task<Result<int>> GetActiveCountAsync();
    Task<Result> AcknowledgeAsync(Guid alertId, Guid userId);
    Task<Result> DismissAsync(Guid alertId, Guid userId);
    Task<Result> CheckLowStockAsync();
    Task<Result> CheckExpiringProductsAsync(int daysAhead = 30);
    Task<Result> CheckCustomerDebtsAsync(decimal threshold = 100);
    Task<Result> RunAllChecksAsync();
}

public record AlertDto(
    Guid Id,
    AlertType Type,
    AlertSeverity Severity,
    AlertStatus Status,
    string Title,
    string Message,
    string? EntityType,
    Guid? EntityId,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    string? AcknowledgedByUserName);

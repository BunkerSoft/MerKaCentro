using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface IAlertRepository : IRepository<Alert>
{
    Task<IReadOnlyList<Alert>> GetActiveAsync();
    Task<IReadOnlyList<Alert>> GetByTypeAsync(AlertType type);
    Task<IReadOnlyList<Alert>> GetBySeverityAsync(AlertSeverity severity);
    Task<Alert?> GetByEntityAsync(string entityType, Guid entityId, AlertType type);
    Task<int> GetActiveCountAsync();
    Task<int> GetActiveCountByTypeAsync(AlertType type);
}

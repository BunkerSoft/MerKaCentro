using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;

namespace MerkaCentro.Domain.Ports.Output;

public interface IAlertRepository : IRepository<Alert, Guid>
{
    Task<IReadOnlyList<Alert>> GetActiveAsync();
    Task<IReadOnlyList<Alert>> GetByTypeAsync(AlertType type);
    Task<IReadOnlyList<Alert>> GetBySeverityAsync(AlertSeverity severity);
    Task<Alert?> GetByEntityAsync(string entityType, Guid entityId, AlertType type);
    Task<int> GetActiveCountAsync();
    Task<int> GetActiveCountByTypeAsync(AlertType type);
}

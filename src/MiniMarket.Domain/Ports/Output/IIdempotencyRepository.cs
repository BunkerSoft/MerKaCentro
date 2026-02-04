using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface IIdempotencyRepository : IRepository<IdempotencyRecord, Guid>
{
    Task<IdempotencyRecord?> GetByKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdempotencyRecord>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}

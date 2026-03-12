using MerkaCentro.Domain.Entities;

namespace MerkaCentro.Domain.Ports.Output;

public interface IUnitOfMeasureRepository : IRepository<UnitOfMeasure, Guid>
{
    Task<IReadOnlyList<UnitOfMeasure>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctGroupsAsync(CancellationToken cancellationToken = default);
}

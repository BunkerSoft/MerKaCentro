using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface ICategoryRepository : IRepository<Category, Guid>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

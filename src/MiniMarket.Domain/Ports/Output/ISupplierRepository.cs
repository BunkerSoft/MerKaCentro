using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface ISupplierRepository : IRepository<Supplier, Guid>
{
    Task<Supplier?> GetByRucAsync(string ruc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> RucExistsAsync(string ruc, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;

namespace MerkaCentro.Domain.Ports.Output;

public interface ICustomerRepository : IRepository<Customer, Guid>
{
    Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetWithDebtAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> DocumentExistsAsync(string documentNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.CreditPayments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.DocumentNumber != null && c.DocumentNumber.Value == documentNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.Status == status)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetWithDebtAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.CurrentDebt.Amount > 0)
            .OrderByDescending(c => c.CurrentDebt.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await DbSet
            .Where(c => c.Name.ToLower().Contains(term) ||
                       (c.DocumentNumber != null && c.DocumentNumber.Value.Contains(term)) ||
                       (c.Phone != null && c.Phone.Value.Contains(term)))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DocumentExistsAsync(string documentNumber, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.DocumentNumber != null && c.DocumentNumber.Value == documentNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

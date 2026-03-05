using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class SupplierRepository : RepositoryBase<Supplier, Guid>, ISupplierRepository
{
    public SupplierRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByRucAsync(string ruc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Ruc != null && s.Ruc.Value == ruc, cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await DbSet
            .Where(s => s.Name.ToLower().Contains(term) ||
                       (s.BusinessName != null && s.BusinessName.ToLower().Contains(term)) ||
                       (s.Ruc != null && s.Ruc.Value.Contains(term)) ||
                       (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(term)))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RucExistsAsync(string ruc, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(s => s.Ruc != null && s.Ruc.Value == ruc);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

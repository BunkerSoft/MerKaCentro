using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class UnitOfMeasureRepository : RepositoryBase<UnitOfMeasure, Guid>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository(MerkaCentroDbContext context) : base(context) { }

    public async Task<IReadOnlyList<UnitOfMeasure>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.DisplayOrder)
            .ThenBy(u => u.Group)
            .ThenBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(u => u.Code == code.ToUpperInvariant());
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive)
            .Select(u => u.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(cancellationToken);
    }
}

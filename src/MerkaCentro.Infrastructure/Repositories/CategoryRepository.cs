using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class CategoryRepository : RepositoryBase<Category, Guid>, ICategoryRepository
{
    public CategoryRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }
}

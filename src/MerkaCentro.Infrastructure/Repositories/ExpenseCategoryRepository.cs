using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class ExpenseCategoryRepository : RepositoryBase<ExpenseCategory, Guid>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<ExpenseCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(ec => ec.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(ec => ec.IsActive)
            .OrderBy(ec => ec.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(ec => ec.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(ec => ec.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasExpensesAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.Expenses.AnyAsync(e => e.CategoryId == categoryId, cancellationToken);
    }
}

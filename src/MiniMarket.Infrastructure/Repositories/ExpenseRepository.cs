using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class ExpenseRepository : RepositoryBase<Expense, Guid>, IExpenseRepository
{
    public ExpenseRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public override async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .Where(e => e.CategoryId == categoryId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .Where(e => e.CreatedAt >= from && e.CreatedAt <= to)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetByCashRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Category)
            .Where(e => e.CashRegisterId == cashRegisterId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.CreatedAt >= from && e.CreatedAt <= to)
            .SumAsync(e => e.Amount.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalByCategoryAsync(Guid categoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.CategoryId == categoryId && e.CreatedAt >= from && e.CreatedAt <= to)
            .SumAsync(e => e.Amount.Amount, cancellationToken);
    }
}

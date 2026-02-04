using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class CashRegisterRepository : RepositoryBase<CashRegister, Guid>, ICashRegisterRepository
{
    public CashRegisterRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public override async Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements.OrderByDescending(m => m.CreatedAt))
            .Include(cr => cr.User)
            .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
    }

    public async Task<CashRegister?> GetOpenByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements)
            .FirstOrDefaultAsync(cr => cr.UserId == userId && cr.Status == CashRegisterStatus.Open, cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements)
            .Where(cr => cr.UserId == userId)
            .OrderByDescending(cr => cr.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements)
            .Include(cr => cr.User)
            .Where(cr => cr.OpenedAt >= from && cr.OpenedAt <= to)
            .OrderByDescending(cr => cr.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByStatusAsync(CashRegisterStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements)
            .Include(cr => cr.User)
            .Where(cr => cr.Status == status)
            .OrderByDescending(cr => cr.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOpenRegisterAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(cr => cr.UserId == userId && cr.Status == CashRegisterStatus.Open, cancellationToken);
    }

    public async Task<CashRegister?> GetWithMovementsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(cr => cr.Movements.OrderByDescending(m => m.CreatedAt))
            .Include(cr => cr.User)
            .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
    }
}

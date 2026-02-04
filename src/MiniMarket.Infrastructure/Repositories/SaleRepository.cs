using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class SaleRepository : RepositoryBase<Sale, Guid>, ISaleRepository
{
    public SaleRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public override async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetByNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Number == saleNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByCashRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => s.CashRegisterId == cashRegisterId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextSaleNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var prefix = $"V{today:yyyyMMdd}";

        var lastSale = await DbSet
            .Where(s => s.Number.StartsWith(prefix))
            .OrderByDescending(s => s.Number)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastSale == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastSale.Number.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    public async Task<decimal> GetTotalSalesByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await DbSet
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt < endDate && s.Status == SaleStatus.Completed)
            .SumAsync(s => s.Total.Amount, cancellationToken);
    }

    public async Task<Sale?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetCreditSalesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Customer)
            .Where(s => s.IsCredit && s.Status == SaleStatus.Completed)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        return await GetNextSaleNumberAsync(cancellationToken);
    }
}

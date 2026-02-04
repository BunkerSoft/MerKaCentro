using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class PurchaseOrderRepository : RepositoryBase<PurchaseOrder, Guid>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public override async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .FirstOrDefaultAsync(po => po.Number == orderNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetBySupplierAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Where(po => po.SupplierId == supplierId)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .Where(po => po.Status == status)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .Where(po => po.CreatedAt >= from && po.CreatedAt <= to)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var prefix = $"OC{today:yyyyMMdd}";

        var lastOrder = await DbSet
            .Where(po => po.Number.StartsWith(prefix))
            .OrderByDescending(po => po.Number)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOrder == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastOrder.Number.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    public async Task<PurchaseOrder?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(po => po.Items)
            .Include(po => po.Supplier)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        return await GetNextOrderNumberAsync(cancellationToken);
    }
}

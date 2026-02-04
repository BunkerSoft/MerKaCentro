using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class ProductRepository : RepositoryBase<Product, Guid>, IProductRepository
{
    public ProductRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Include(p => p.StockMovements.OrderByDescending(m => m.CreatedAt).Take(10))
            .Include(p => p.PriceHistory.OrderByDescending(h => h.EffectiveDate).Take(10))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode != null && p.Barcode.Value == barcode, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.Status == status)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.CurrentStock.Value <= p.MinStock.Value && p.Status == ProductStatus.Active)
            .OrderBy(p => p.CurrentStock.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(term) ||
                       p.Code.ToLower().Contains(term) ||
                       (p.Barcode != null && p.Barcode.Value.Contains(term)) ||
                       (p.Description != null && p.Description.ToLower().Contains(term)))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Code == code);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Barcode != null && p.Barcode.Value == barcode);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasSalesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await Context.SaleItems.AnyAsync(si => si.ProductId == productId, cancellationToken);
    }
}

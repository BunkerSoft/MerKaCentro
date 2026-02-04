using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasSalesAsync(Guid productId, CancellationToken cancellationToken = default);
}

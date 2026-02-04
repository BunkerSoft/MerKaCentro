using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(Guid id);
    Task<Result<ProductDto>> GetByBarcodeAsync(string barcode);
    Task<Result<PagedResult<ProductDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<PagedResult<ProductDto>>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<ProductDto>>> GetByCategoryAsync(Guid categoryId);
    Task<Result<IEnumerable<ProductDto>>> GetLowStockAsync();
    Task<Result<IEnumerable<ProductDto>>> GetActiveAsync();
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> UpdateStockAsync(Guid id, decimal quantity, string reason);
    Task<Result> UpdatePriceAsync(Guid id, decimal newPrice, string? reason = null);
}

public record CreateProductDto(
    string Name,
    string? Description,
    string? Barcode,
    decimal PurchasePrice,
    decimal SalePrice,
    string Currency,
    decimal CurrentStock,
    decimal MinStock,
    string UnitOfMeasure,
    Guid CategoryId);

public record UpdateProductDto(
    string Name,
    string? Description,
    string? Barcode,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal MinStock,
    string UnitOfMeasure,
    Guid CategoryId);

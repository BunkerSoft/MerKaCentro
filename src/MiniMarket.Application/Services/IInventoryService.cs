using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface IInventoryService
{
    Task<Result<IEnumerable<StockMovementDto>>> GetMovementsByProductAsync(Guid productId);
    Task<Result<PagedResult<StockMovementDto>>> GetAllMovementsAsync(int page = 1, int pageSize = 20);
    Task<Result<PagedResult<StockMovementDto>>> GetMovementsByDateRangeAsync(DateTime from, DateTime to, int page = 1, int pageSize = 20);
    Task<Result<StockMovementDto>> RegisterEntryAsync(StockEntryDto dto);
    Task<Result<StockMovementDto>> RegisterExitAsync(StockExitDto dto);
    Task<Result<StockMovementDto>> RegisterAdjustmentAsync(StockAdjustmentDto dto);
    Task<Result<IEnumerable<ProductStockDto>>> GetLowStockProductsAsync();
    Task<Result<IEnumerable<ProductStockDto>>> GetOutOfStockProductsAsync();
    Task<Result<decimal>> GetCurrentStockAsync(Guid productId);
    Task<Result<InventoryValueDto>> GetInventoryValueAsync();
}

public record StockMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    MovementType Type,
    decimal Quantity,
    decimal PreviousStock,
    decimal NewStock,
    string? Reference,
    string? Notes,
    DateTime CreatedAt);

public record StockEntryDto(
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes);

public record StockExitDto(
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes);

public record StockAdjustmentDto(
    Guid ProductId,
    decimal NewStock,
    string Reason);

public record ProductStockDto(
    Guid ProductId,
    string ProductName,
    string? Barcode,
    decimal CurrentStock,
    decimal MinStock,
    string UnitOfMeasure,
    bool IsLowStock,
    bool IsOutOfStock);

public record InventoryValueDto(
    decimal TotalPurchaseValue,
    decimal TotalSaleValue,
    decimal PotentialProfit,
    int TotalProducts,
    int LowStockProducts,
    int OutOfStockProducts,
    string Currency);

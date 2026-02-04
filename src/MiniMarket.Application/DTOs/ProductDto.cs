using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Code,
    string? Barcode,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    decimal PurchasePrice,
    decimal SalePrice,
    string Currency,
    decimal MinStock,
    decimal CurrentStock,
    string Unit,
    bool AllowFractions,
    ProductStatus Status,
    decimal ProfitMargin,
    bool IsLowStock,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateProductDto(
    string Code,
    string? Barcode,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal PurchasePrice,
    decimal SalePrice,
    string Unit,
    decimal MinStock = 5,
    bool AllowFractions = false);

public record UpdateProductDto(
    string Name,
    string? Barcode,
    string? Description,
    Guid CategoryId,
    string Unit,
    decimal MinStock,
    bool AllowFractions);

public record UpdateProductPricesDto(
    decimal PurchasePrice,
    decimal SalePrice);

public record ProductStockDto(
    Guid Id,
    string Code,
    string Name,
    decimal CurrentStock,
    decimal MinStock,
    string Unit,
    bool IsLowStock);

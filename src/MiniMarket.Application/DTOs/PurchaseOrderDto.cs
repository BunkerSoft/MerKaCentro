using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record PurchaseOrderDto(
    Guid Id,
    string Number,
    Guid SupplierId,
    string SupplierName,
    Guid UserId,
    string UserName,
    decimal Total,
    PurchaseOrderStatus Status,
    DateTime? ReceivedAt,
    string? Notes,
    IReadOnlyList<PurchaseOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal ReceivedQuantity,
    decimal PendingQuantity,
    decimal UnitCost,
    decimal Total,
    bool IsFullyReceived);

public record CreatePurchaseOrderDto(
    Guid SupplierId,
    string? Notes,
    IList<CreatePurchaseOrderItemDto> Items);

public record CreatePurchaseOrderItemDto(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost);

public record ReceiveItemDto(
    Guid ProductId,
    decimal ReceivedQuantity);

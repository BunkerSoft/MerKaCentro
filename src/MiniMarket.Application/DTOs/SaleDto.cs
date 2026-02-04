using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record SaleDto(
    Guid Id,
    string Number,
    Guid? CustomerId,
    string? CustomerName,
    Guid CashRegisterId,
    Guid UserId,
    string UserName,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    PaymentMethod PaymentMethod,
    decimal AmountPaid,
    decimal Change,
    SaleStatus Status,
    bool IsCredit,
    string? Notes,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<SalePaymentDto> Payments,
    DateTime CreatedAt);

public record SaleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total);

public record SalePaymentDto(
    Guid Id,
    PaymentMethod Method,
    decimal Amount,
    string? Reference);

public record CreateSaleDto(
    Guid? CustomerId,
    Guid CashRegisterId,
    bool IsCredit,
    string? Notes,
    IList<CreateSaleItemDto> Items,
    IList<CreateSalePaymentDto> Payments);

public record CreateSaleItemDto(
    Guid ProductId,
    decimal Quantity,
    decimal? UnitPrice,
    decimal? DiscountPercent);

public record CreateSalePaymentDto(
    PaymentMethod Method,
    decimal Amount,
    string? Reference);

public record SaleSummaryDto(
    int TotalSales,
    decimal TotalAmount,
    decimal TotalCash,
    decimal TotalCard,
    decimal TotalOther,
    decimal TotalCredit);

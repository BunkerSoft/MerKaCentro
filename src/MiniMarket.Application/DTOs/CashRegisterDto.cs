using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record CashRegisterDto(
    Guid Id,
    Guid UserId,
    string UserName,
    decimal InitialCash,
    decimal CurrentCash,
    decimal ExpectedCash,
    decimal? FinalCash,
    decimal? Difference,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    CashRegisterStatus Status,
    string? Notes,
    CashRegisterSummaryDto Summary);

public record CashRegisterSummaryDto(
    decimal TotalSales,
    decimal TotalExpenses,
    decimal TotalWithdrawals,
    decimal TotalDeposits,
    decimal TotalCreditPayments,
    int SalesCount);

public record CashMovementDto(
    Guid Id,
    CashMovementType Type,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTime CreatedAt);

public record OpenCashRegisterDto(
    decimal InitialCash,
    string? Notes);

public record CloseCashRegisterDto(
    decimal FinalCash,
    string? Notes);

public record CashWithdrawalDto(
    decimal Amount,
    string Reason);

public record CashDepositDto(
    decimal Amount,
    string Reason);

using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface ICashRegisterService
{
    Task<Result<CashRegisterDto>> GetByIdAsync(Guid id);
    Task<Result<CashRegisterDto>> GetCurrentOpenAsync(Guid userId);
    Task<Result<PagedResult<CashRegisterDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<CashRegisterDto>>> GetByUserAsync(Guid userId);
    Task<Result<IEnumerable<CashRegisterDto>>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<Result<CashRegisterDto>> OpenAsync(OpenCashRegisterDto dto);
    Task<Result<CashRegisterDto>> CloseAsync(Guid id, decimal countedCash);
    Task<Result<CashRegisterDto>> RegisterSaleAsync(Guid id, decimal amount, string currency = "PEN");
    Task<Result<CashRegisterDto>> RegisterWithdrawalAsync(Guid id, RegisterMovementDto dto);
    Task<Result<CashRegisterDto>> RegisterDepositAsync(Guid id, RegisterMovementDto dto);
    Task<Result<CashRegisterDto>> RegisterExpenseAsync(Guid id, RegisterExpenseDto dto);
    Task<Result<CashRegisterFullSummaryDto>> GetSummaryAsync(Guid id);
}

public record OpenCashRegisterDto(
    Guid UserId,
    decimal InitialCash,
    string Currency = "PEN");

public record RegisterMovementDto(
    decimal Amount,
    string Currency,
    string Description,
    Guid? AuthorizedBy = null);

public record RegisterExpenseDto(
    decimal Amount,
    string Currency,
    string Description,
    Guid ExpenseCategoryId);

public record CashRegisterFullSummaryDto(
    Guid Id,
    decimal InitialCash,
    decimal TotalSales,
    decimal TotalDeposits,
    decimal TotalWithdrawals,
    decimal TotalExpenses,
    decimal ExpectedCash,
    decimal? CountedCash,
    decimal? Difference,
    int TotalTransactions,
    string Currency);

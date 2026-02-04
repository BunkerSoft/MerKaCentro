using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface IExpenseService
{
    Task<Result<ExpenseDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<ExpenseDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<ExpenseDto>>> GetByCashRegisterAsync(Guid cashRegisterId);
    Task<Result<IEnumerable<ExpenseDto>>> GetByCategoryAsync(Guid categoryId);
    Task<Result<IEnumerable<ExpenseDto>>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<Result<IEnumerable<ExpenseDto>>> GetTodayExpensesAsync();
    Task<Result<ExpenseDto>> CreateAsync(CreateExpenseDto dto, Guid userId);
    Task<Result<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<decimal>> GetTotalByDateRangeAsync(DateTime from, DateTime to);
    Task<Result<decimal>> GetTotalByCategoryAsync(Guid categoryId, DateTime from, DateTime to);
}

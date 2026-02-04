using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface IExpenseCategoryService
{
    Task<Result<ExpenseCategoryDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<ExpenseCategoryDto>>> GetAllAsync();
    Task<Result<IEnumerable<ExpenseCategoryDto>>> GetActiveAsync();
    Task<Result<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryDto dto);
    Task<Result<ExpenseCategoryDto>> UpdateAsync(Guid id, UpdateExpenseCategoryDto dto);
    Task<Result> ActivateAsync(Guid id);
    Task<Result> DeactivateAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}

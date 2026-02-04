using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<CategoryDto>>> GetAllAsync();
    Task<Result<IEnumerable<CategoryDto>>> GetActiveAsync();
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ActivateAsync(Guid id);
    Task<Result> DeactivateAsync(Guid id);
}

public record CreateCategoryDto(string Name, string? Description);
public record UpdateCategoryDto(string Name, string? Description);

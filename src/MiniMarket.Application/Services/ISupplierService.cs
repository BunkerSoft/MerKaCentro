using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface ISupplierService
{
    Task<Result<SupplierDto>> GetByIdAsync(Guid id);
    Task<Result<SupplierDto>> GetByRucAsync(string ruc);
    Task<Result<PagedResult<SupplierDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<SupplierDto>>> GetActiveAsync();
    Task<Result<IEnumerable<SupplierDto>>> SearchAsync(string searchTerm);
    Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto dto);
    Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierDto dto);
    Task<Result<SupplierDto>> ActivateAsync(Guid id);
    Task<Result<SupplierDto>> DeactivateAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}

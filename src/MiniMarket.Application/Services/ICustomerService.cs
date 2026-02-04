using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(Guid id);
    Task<Result<CustomerDto>> GetByDocumentAsync(string documentNumber);
    Task<Result<PagedResult<CustomerDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<CustomerDto>>> GetActiveAsync();
    Task<Result<IEnumerable<CustomerDebtDto>>> GetWithDebtAsync();
    Task<Result<IEnumerable<CustomerDto>>> SearchAsync(string searchTerm);
    Task<Result<CustomerDto>> CreateAsync(CreateCustomerDto dto);
    Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task<Result<CustomerDto>> SetCreditLimitAsync(Guid id, decimal creditLimit);
    Task<Result<CustomerDto>> ActivateAsync(Guid id);
    Task<Result<CustomerDto>> DeactivateAsync(Guid id);
    Task<Result<CustomerDto>> BlockAsync(Guid id, string? reason = null);
    Task<Result> DeleteAsync(Guid id);
}

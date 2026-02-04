using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface ISaleService
{
    Task<Result<SaleDto>> GetByIdAsync(Guid id);
    Task<Result<SaleDto>> GetByNumberAsync(string saleNumber);
    Task<Result<PagedResult<SaleDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<PagedResult<SaleDto>>> GetByDateRangeAsync(DateTime from, DateTime to, int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<SaleDto>>> GetByCustomerAsync(Guid customerId);
    Task<Result<IEnumerable<SaleDto>>> GetByCashRegisterAsync(Guid cashRegisterId);
    Task<Result<IEnumerable<SaleDto>>> GetTodaySalesAsync();
    Task<Result<SaleDto>> CreateAsync(CreateSaleDto dto, Guid userId);
    Task<Result<SaleDto>> CancelAsync(Guid saleId, string reason);
    Task<Result<SaleSummaryDto>> GetDailySummaryAsync(DateTime date);
    Task<Result<decimal>> GetDailySalesTotal(DateTime date);
}

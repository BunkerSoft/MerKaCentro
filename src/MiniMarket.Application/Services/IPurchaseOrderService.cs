using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface IPurchaseOrderService
{
    Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id);
    Task<Result<PurchaseOrderDto>> GetByNumberAsync(string number);
    Task<Result<PagedResult<PurchaseOrderDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Result<IEnumerable<PurchaseOrderDto>>> GetBySupplierAsync(Guid supplierId);
    Task<Result<IEnumerable<PurchaseOrderDto>>> GetByStatusAsync(PurchaseOrderStatus status);
    Task<Result<IEnumerable<PurchaseOrderDto>>> GetPendingAsync();
    Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto dto, Guid userId);
    Task<Result<PurchaseOrderDto>> ReceiveItemsAsync(Guid id, IEnumerable<ReceiveItemDto> items);
    Task<Result<PurchaseOrderDto>> MarkAsReceivedAsync(Guid id);
    Task<Result<PurchaseOrderDto>> CancelAsync(Guid id, string? reason = null);
}

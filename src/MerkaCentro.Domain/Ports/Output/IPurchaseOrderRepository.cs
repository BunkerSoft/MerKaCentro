using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;

namespace MerkaCentro.Domain.Ports.Output;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder, Guid>
{
    Task<PurchaseOrder?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrder>> GetBySupplierAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}

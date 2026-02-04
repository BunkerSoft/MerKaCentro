using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface ISaleRepository : IRepository<Sale, Guid>
{
    Task<Sale?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<Sale?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetByCashRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetByStatusAsync(SaleStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetCreditSalesAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}

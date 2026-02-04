using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface ICashRegisterRepository : IRepository<CashRegister, Guid>
{
    Task<CashRegister?> GetOpenByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CashRegister?> GetWithMovementsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetByStatusAsync(CashRegisterStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashRegister>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> HasOpenRegisterAsync(Guid userId, CancellationToken cancellationToken = default);
}

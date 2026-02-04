using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface IExpenseRepository : IRepository<Expense, Guid>
{
    Task<IReadOnlyList<Expense>> GetByCashRegisterAsync(Guid cashRegisterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

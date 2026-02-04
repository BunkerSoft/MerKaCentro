using MiniMarket.Domain.Entities;

namespace MiniMarket.Domain.Ports.Output;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory, Guid>
{
    Task<ExpenseCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasExpensesAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

namespace MiniMarket.Domain.Ports.Output;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    ISupplierRepository Suppliers { get; }
    ISaleRepository Sales { get; }
    ICashRegisterRepository CashRegisters { get; }
    IUserRepository Users { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IExpenseRepository Expenses { get; }
    IExpenseCategoryRepository ExpenseCategories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

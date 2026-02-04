using Microsoft.EntityFrameworkCore.Storage;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MiniMarketDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        MiniMarketDbContext context,
        ICategoryRepository categories,
        IProductRepository products,
        ICustomerRepository customers,
        ISupplierRepository suppliers,
        ISaleRepository sales,
        ICashRegisterRepository cashRegisters,
        IUserRepository users,
        IPurchaseOrderRepository purchaseOrders,
        IExpenseRepository expenses,
        IExpenseCategoryRepository expenseCategories)
    {
        _context = context;
        Categories = categories;
        Products = products;
        Customers = customers;
        Suppliers = suppliers;
        Sales = sales;
        CashRegisters = cashRegisters;
        Users = users;
        PurchaseOrders = purchaseOrders;
        Expenses = expenses;
        ExpenseCategories = expenseCategories;
    }

    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public ICustomerRepository Customers { get; }
    public ISupplierRepository Suppliers { get; }
    public ISaleRepository Sales { get; }
    public ICashRegisterRepository CashRegisters { get; }
    public IUserRepository Users { get; }
    public IPurchaseOrderRepository PurchaseOrders { get; }
    public IExpenseRepository Expenses { get; }
    public IExpenseCategoryRepository ExpenseCategories { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

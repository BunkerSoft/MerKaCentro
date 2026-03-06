using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MerkaCentroDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        MerkaCentroDbContext context,
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
        FixOwnedEntityStates();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Fixes EF Core change tracker issues with immutable owned entities (value objects).
    ///
    /// Problem 1: Replacing an owned entity instance (e.g. CurrentStock = CurrentStock.Subtract(x))
    /// causes EF to create a Deleted entry for the old instance and an Added entry for the new one.
    /// For table-split owned types this is invalid — we need a single Modified entry.
    ///
    /// Problem 2: New child entities added to navigation collections (e.g. StockMovement, CashMovement)
    /// are discovered by DetectChanges as Modified instead of Added.
    /// </summary>
    private void FixOwnedEntityStates()
    {
        _context.ChangeTracker.DetectChanges();

        var entries = _context.ChangeTracker.Entries().ToList();

        // Fix 1: Merge Deleted+Added pairs of owned entities into a single Modified entry
        var ownedEntries = entries.Where(e => e.Metadata.IsOwned()).ToList();
        var deletedOwned = ownedEntries.Where(e => e.State == EntityState.Deleted).ToList();

        foreach (var deleted in deletedOwned)
        {
            var added = ownedEntries.FirstOrDefault(e =>
                e.State == EntityState.Added &&
                e.Metadata.Name == deleted.Metadata.Name &&
                PrimaryKeysMatch(e, deleted));

            if (added == null) continue;

            // Capture current values from Added entry before detaching
            var newValues = new Dictionary<string, object?>();
            foreach (var prop in added.Properties)
            {
                if (!prop.Metadata.IsPrimaryKey())
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
            }

            // Detach Added entry FIRST to free the key
            added.State = EntityState.Detached;

            // Now update the Deleted entry with new values and set to Modified
            foreach (var prop in deleted.Properties)
            {
                if (prop.Metadata.IsPrimaryKey()) continue;
                if (newValues.TryGetValue(prop.Metadata.Name, out var newVal))
                {
                    prop.CurrentValue = newVal;
                    prop.IsModified = !Equals(prop.OriginalValue, prop.CurrentValue);
                }
            }
            deleted.State = EntityState.Modified;
        }

        // Fix 2: Entities discovered in navigation collections tracked as Modified
        // but never loaded from DB should be Added.
        // Detection: all "modified" properties have identical original and current values.
        foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
        {
            if (entry.Metadata.IsOwned()) continue;

            var modifiedProps = entry.Properties.Where(p => p.IsModified).ToList();
            if (modifiedProps.Count > 0 && modifiedProps.All(p => Equals(p.OriginalValue, p.CurrentValue)))
            {
                entry.State = EntityState.Added;
            }
        }
    }

    private static bool PrimaryKeysMatch(EntityEntry a, EntityEntry b)
    {
        var aPks = a.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();
        var bPks = b.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();

        if (aPks.Count != bPks.Count) return false;

        return aPks.All(ap =>
        {
            var bp = bPks.FirstOrDefault(p => p.Metadata.Name == ap.Metadata.Name);
            return bp != null && Equals(ap.CurrentValue, bp.CurrentValue);
        });
    }

    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
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

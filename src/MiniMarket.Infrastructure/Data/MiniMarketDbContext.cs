using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;

namespace MiniMarket.Infrastructure.Data;

public class MiniMarketDbContext : DbContext
{
    public MiniMarketDbContext(DbContextOptions<MiniMarketDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CreditPayment> CreditPayments => Set<CreditPayment>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    // Stage 4 - Advanced Features
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<SyncQueueItem> SyncQueue => Set<SyncQueueItem>();

    // Stage 5 - Polish
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniMarketDbContext).Assembly);
    }
}

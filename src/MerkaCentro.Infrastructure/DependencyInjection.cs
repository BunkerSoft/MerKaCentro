using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MerkaCentro.Application.Services;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;
using MerkaCentro.Infrastructure.Repositories;
using MerkaCentro.Infrastructure.Services;

namespace MerkaCentro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MerkaCentroDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(MerkaCentroDbContext).Assembly.FullName)));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();

        // Stage 4 - Advanced Features
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<ISyncQueueRepository, SyncQueueRepository>();

        // Catalogs
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();

        // Stage 5 - Polish
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<MerkaCentro.Application.Services.IBackupService, BackupService>();

        // POS Printing and PDF services
        services.AddScoped<ITicketPrinterService, EscPosTicketPrinterService>();
        services.AddScoped<ISaleInvoicePdfService, SaleInvoicePdfService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

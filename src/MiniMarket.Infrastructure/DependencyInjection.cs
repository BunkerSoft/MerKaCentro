using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;
using MiniMarket.Infrastructure.Repositories;

namespace MiniMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MiniMarketDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(MiniMarketDbContext).Assembly.FullName)));

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

        // Stage 5 - Polish
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

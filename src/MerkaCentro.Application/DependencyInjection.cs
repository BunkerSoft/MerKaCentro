using Microsoft.Extensions.DependencyInjection;
using MerkaCentro.Application.Mappers;
using MerkaCentro.Application.Services;

namespace MerkaCentro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // Catalogos
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

        // Fase 2 - Módulos Core
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();

        // Fase 3 - Módulos Secundarios
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        // Fase 4 - Funcionalidades Avanzadas
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        // ITicketPrinterService is registered in Infrastructure (ESC/POS)

        // Fase 5 - Pulido y Produccion
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        // IBackupService is registered in Infrastructure

        return services;
    }
}

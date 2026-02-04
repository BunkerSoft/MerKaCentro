using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface IReportService
{
    Task<Result<DashboardDto>> GetDashboardAsync();
    Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime from, DateTime to);
    Task<Result<InventoryReportDto>> GetInventoryReportAsync();
    Task<Result<ProfitabilityReportDto>> GetProfitabilityReportAsync(DateTime from, DateTime to, int topN = 10);
}

public record DashboardDto(
    decimal TodaySales,
    int TodayTransactions,
    decimal MonthSales,
    int MonthTransactions,
    decimal TodayExpenses,
    decimal AvailableCash,
    int LowStockProducts,
    int PendingPurchaseOrders,
    decimal TotalCustomerDebt,
    int CustomersWithDebt,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<RecentSaleDto> RecentSales);

public record TopProductDto(
    Guid Id,
    string Name,
    decimal QuantitySold,
    decimal TotalSales);

public record RecentSaleDto(
    Guid Id,
    string Number,
    decimal Total,
    DateTime Date,
    string? CustomerName);

public record SalesReportDto(
    DateTime From,
    DateTime To,
    decimal TotalSales,
    int TotalTransactions,
    decimal AverageTicket,
    decimal TotalDiscount,
    IReadOnlyList<DailySalesDto> DailySales,
    IReadOnlyList<PaymentMethodSummaryDto> PaymentMethods,
    IReadOnlyList<CategorySalesDto> SalesByCategory);

public record DailySalesDto(
    DateTime Date,
    decimal Total,
    int Transactions);

public record PaymentMethodSummaryDto(
    string Method,
    decimal Total,
    int Count);

public record CategorySalesDto(
    string Category,
    decimal Total,
    int ProductsSold);

public record InventoryReportDto(
    int TotalProducts,
    int ActiveProducts,
    int LowStockProducts,
    decimal TotalInventoryValue,
    IReadOnlyList<LowStockProductDto> LowStockList,
    IReadOnlyList<CategoryInventoryDto> InventoryByCategory);

public record LowStockProductDto(
    Guid Id,
    string Name,
    decimal CurrentStock,
    decimal MinStock,
    string Unit);

public record CategoryInventoryDto(
    string Category,
    int ProductCount,
    decimal TotalValue);

public record ProfitabilityReportDto(
    DateTime From,
    DateTime To,
    decimal TotalRevenue,
    decimal TotalCost,
    decimal GrossProfit,
    decimal GrossMargin,
    IReadOnlyList<ProductProfitabilityDto> TopProfitableProducts,
    IReadOnlyList<ProductProfitabilityDto> LowProfitableProducts);

public record ProductProfitabilityDto(
    Guid Id,
    string Name,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    decimal Margin);

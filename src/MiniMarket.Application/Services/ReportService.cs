using MiniMarket.Application.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class ReportService : IReportService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public ReportService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICustomerRepository customerRepository,
        ICashRegisterRepository cashRegisterRepository,
        IExpenseRepository expenseRepository,
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _customerRepository = customerRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _expenseRepository = expenseRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<Result<DashboardDto>> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Today's sales
        var todaySales = await _saleRepository.GetByDateRangeAsync(today, tomorrow);
        var completedTodaySales = todaySales.Where(s => s.Status == SaleStatus.Completed).ToList();
        var todayTotal = completedTodaySales.Sum(s => s.Total.Amount);
        var todayTransactions = completedTodaySales.Count;

        // Month sales
        var monthSales = await _saleRepository.GetByDateRangeAsync(monthStart, tomorrow);
        var completedMonthSales = monthSales.Where(s => s.Status == SaleStatus.Completed).ToList();
        var monthTotal = completedMonthSales.Sum(s => s.Total.Amount);
        var monthTransactions = completedMonthSales.Count;

        // Today's expenses
        var todayExpenses = await _expenseRepository.GetByDateRangeAsync(today, tomorrow);
        var todayExpenseTotal = todayExpenses.Sum(e => e.Amount.Amount);

        // Available cash (open cash registers)
        var allCashRegisters = await _cashRegisterRepository.GetAllAsync();
        var openRegisters = allCashRegisters.Where(c => c.Status == CashRegisterStatus.Open).ToList();
        var availableCash = openRegisters.Sum(c => c.CurrentCash.Amount);

        // Low stock products
        var lowStockProducts = await _productRepository.GetLowStockAsync();
        var lowStockCount = lowStockProducts.Count;

        // Pending purchase orders
        var pendingOrders = await _purchaseOrderRepository.GetByStatusAsync(PurchaseOrderStatus.Pending);
        var partialOrders = await _purchaseOrderRepository.GetByStatusAsync(PurchaseOrderStatus.PartiallyReceived);
        var pendingOrderCount = pendingOrders.Count + partialOrders.Count;

        // Customer debt
        var customersWithDebt = await _customerRepository.GetWithDebtAsync();
        var totalDebt = customersWithDebt.Sum(c => c.CurrentDebt.Amount);
        var debtCustomerCount = customersWithDebt.Count;

        // Top products (last 30 days)
        var last30Days = today.AddDays(-30);
        var recentSales = await _saleRepository.GetByDateRangeAsync(last30Days, tomorrow);
        var productSales = recentSales
            .Where(s => s.Status == SaleStatus.Completed)
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(i => i.Quantity.Value),
                g.Sum(i => i.Total.Amount)))
            .OrderByDescending(p => p.TotalSales)
            .Take(5)
            .ToList();

        // Recent sales
        var recentSalesList = todaySales
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new RecentSaleDto(
                s.Id,
                s.Number,
                s.Total.Amount,
                s.CreatedAt,
                s.Customer?.Name))
            .ToList();

        var dashboard = new DashboardDto(
            todayTotal,
            todayTransactions,
            monthTotal,
            monthTransactions,
            todayExpenseTotal,
            availableCash,
            lowStockCount,
            pendingOrderCount,
            totalDebt,
            debtCustomerCount,
            productSales,
            recentSalesList);

        return Result<DashboardDto>.Success(dashboard);
    }

    public async Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var toEndOfDay = to.Date.AddDays(1);
        var sales = await _saleRepository.GetByDateRangeAsync(from.Date, toEndOfDay);
        var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();

        var totalSales = completedSales.Sum(s => s.Total.Amount);
        var totalTransactions = completedSales.Count;
        var averageTicket = totalTransactions > 0 ? totalSales / totalTransactions : 0;
        var totalDiscount = completedSales.Sum(s => s.Discount.Amount);

        // Daily breakdown
        var dailySales = completedSales
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new DailySalesDto(
                g.Key,
                g.Sum(s => s.Total.Amount),
                g.Count()))
            .OrderBy(d => d.Date)
            .ToList();

        // Payment methods
        var paymentMethods = completedSales
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSummaryDto(
                g.Key.ToString(),
                g.Sum(p => p.Amount.Amount),
                g.Count()))
            .ToList();

        // Sales by category
        var categories = await _categoryRepository.GetAllAsync();
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var products = await _productRepository.GetAllAsync();
        var productCategoryMap = products.ToDictionary(p => p.Id, p => p.CategoryId);

        var salesByCategory = completedSales
            .SelectMany(s => s.Items)
            .GroupBy(i => productCategoryMap.GetValueOrDefault(i.ProductId))
            .Select(g => new CategorySalesDto(
                categoryMap.GetValueOrDefault(g.Key, "Sin Categoria"),
                g.Sum(i => i.Total.Amount),
                g.Sum(i => (int)i.Quantity.Value)))
            .OrderByDescending(c => c.Total)
            .ToList();

        var report = new SalesReportDto(
            from.Date,
            to.Date,
            totalSales,
            totalTransactions,
            averageTicket,
            totalDiscount,
            dailySales,
            paymentMethods,
            salesByCategory);

        return Result<SalesReportDto>.Success(report);
    }

    public async Task<Result<InventoryReportDto>> GetInventoryReportAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.Status == ProductStatus.Active).ToList();
        var lowStockProducts = products.Where(p => p.IsLowStock()).ToList();

        var totalValue = activeProducts.Sum(p => p.CurrentStock.Value * p.PurchasePrice.Amount);

        var lowStockList = lowStockProducts
            .OrderBy(p => p.CurrentStock.Value - p.MinStock.Value)
            .Take(20)
            .Select(p => new LowStockProductDto(
                p.Id,
                p.Name,
                p.CurrentStock.Value,
                p.MinStock.Value,
                p.Unit))
            .ToList();

        var categories = await _categoryRepository.GetAllAsync();
        var inventoryByCategory = products
            .GroupBy(p => p.CategoryId)
            .Select(g =>
            {
                var category = categories.FirstOrDefault(c => c.Id == g.Key);
                return new CategoryInventoryDto(
                    category?.Name ?? "Sin Categoria",
                    g.Count(),
                    g.Sum(p => p.CurrentStock.Value * p.PurchasePrice.Amount));
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();

        var report = new InventoryReportDto(
            products.Count,
            activeProducts.Count,
            lowStockProducts.Count,
            totalValue,
            lowStockList,
            inventoryByCategory);

        return Result<InventoryReportDto>.Success(report);
    }

    public async Task<Result<ProfitabilityReportDto>> GetProfitabilityReportAsync(DateTime from, DateTime to, int topN = 10)
    {
        var toEndOfDay = to.Date.AddDays(1);
        var sales = await _saleRepository.GetByDateRangeAsync(from.Date, toEndOfDay);
        var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();
        var products = await _productRepository.GetAllAsync();
        var productCostMap = products.ToDictionary(p => p.Id, p => p.PurchasePrice.Amount);

        var productProfitability = completedSales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g =>
            {
                var revenue = g.Sum(i => i.Total.Amount);
                var cost = g.Sum(i => i.Quantity.Value * productCostMap.GetValueOrDefault(i.ProductId, 0));
                var profit = revenue - cost;
                var margin = revenue > 0 ? (profit / revenue) * 100 : 0;

                return new ProductProfitabilityDto(
                    g.Key.ProductId,
                    g.Key.ProductName,
                    revenue,
                    cost,
                    profit,
                    margin);
            })
            .ToList();

        var totalRevenue = productProfitability.Sum(p => p.Revenue);
        var totalCost = productProfitability.Sum(p => p.Cost);
        var grossProfit = totalRevenue - totalCost;
        var grossMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;

        var topProfitable = productProfitability
            .OrderByDescending(p => p.Profit)
            .Take(topN)
            .ToList();

        var lowProfitable = productProfitability
            .OrderBy(p => p.Margin)
            .Take(topN)
            .ToList();

        var report = new ProfitabilityReportDto(
            from.Date,
            to.Date,
            totalRevenue,
            totalCost,
            grossProfit,
            grossMargin,
            topProfitable,
            lowProfitable);

        return Result<ProfitabilityReportDto>.Success(report);
    }
}

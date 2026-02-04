using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IReportService _reportService;
    private readonly ISyncService _syncService;

    public DashboardController(
        IReportService reportService,
        ISyncService syncService)
    {
        _reportService = reportService;
        _syncService = syncService;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardResult = await _reportService.GetDashboardAsync();
        if (!dashboardResult.IsSuccess)
        {
            TempData["Error"] = dashboardResult.Error;
        }

        var syncStatus = await _syncService.GetStatusAsync();
        ViewBag.SyncStatus = syncStatus.Value;

        return View(dashboardResult.Value);
    }

    public async Task<IActionResult> SalesReport(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddDays(-30);
        var toDate = to ?? DateTime.Today;

        var result = await _reportService.GetSalesReportAsync(fromDate, toDate);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.From = fromDate.ToString("yyyy-MM-dd");
        ViewBag.To = toDate.ToString("yyyy-MM-dd");

        return View(result.Value);
    }

    public async Task<IActionResult> InventoryReport()
    {
        var result = await _reportService.GetInventoryReportAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        return View(result.Value);
    }

    public async Task<IActionResult> ProfitabilityReport(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddDays(-30);
        var toDate = to ?? DateTime.Today;

        var result = await _reportService.GetProfitabilityReportAsync(fromDate, toDate);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.From = fromDate.ToString("yyyy-MM-dd");
        ViewBag.To = toDate.ToString("yyyy-MM-dd");

        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var result = await _reportService.GetDashboardAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }
        return Json(result.Value);
    }
}

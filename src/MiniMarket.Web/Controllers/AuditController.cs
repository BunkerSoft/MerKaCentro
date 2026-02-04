using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(int page = 1, string? entityType = null)
    {
        var result = await _auditService.GetPagedAsync(page, 50, entityType);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View();
        }

        ViewBag.EntityType = entityType;
        return View(result.Value);
    }

    public async Task<IActionResult> Entity(string type, Guid id)
    {
        var result = await _auditService.GetByEntityAsync(type, id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.EntityType = type;
        ViewBag.EntityId = id;
        return View(result.Value);
    }

    public async Task<IActionResult> User(Guid id)
    {
        var result = await _auditService.GetByUserAsync(id, 100);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.UserId = id;
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cleanup(int daysToKeep = 90)
    {
        var result = await _auditService.CleanupOldLogsAsync(daysToKeep);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = $"Logs anteriores a {daysToKeep} dias eliminados";
        }

        return RedirectToAction(nameof(Index));
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.Services;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Web.Controllers;

[Authorize]
public class AlertsController : Controller
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _alertService.GetActiveAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View(Array.Empty<AlertDto>());
        }

        return View(result.Value);
    }

    public async Task<IActionResult> ByType(AlertType type)
    {
        var result = await _alertService.GetByTypeAsync(type);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View("Index", Array.Empty<AlertDto>());
        }

        ViewBag.FilterType = type;
        return View("Index", result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _alertService.AcknowledgeAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Alerta reconocida";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dismiss(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _alertService.DismissAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Alerta descartada";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunChecks()
    {
        var result = await _alertService.RunAllChecksAsync();

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Verificaciones ejecutadas";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        var result = await _alertService.GetActiveCountAsync();
        return Json(new { count = result.Value });
    }
}

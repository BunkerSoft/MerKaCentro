using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

[Authorize(Roles = "Admin")]
public class BackupController : Controller
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _backupService.GetBackupsAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View(Array.Empty<BackupInfo>());
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? customName = null)
    {
        var result = await _backupService.CreateBackupAsync(customName);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = $"Backup creado: {Path.GetFileName(result.Value)}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string backupPath)
    {
        var result = await _backupService.RestoreBackupAsync(backupPath);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Base de datos restaurada exitosamente";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string backupPath)
    {
        var result = await _backupService.DeleteBackupAsync(backupPath);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Backup eliminado";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cleanup(int keepCount = 10)
    {
        var result = await _backupService.CleanupOldBackupsAsync(keepCount);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = $"Backups antiguos eliminados (se mantienen los ultimos {keepCount})";
        }

        return RedirectToAction(nameof(Index));
    }
}

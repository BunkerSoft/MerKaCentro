using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MerkaCentro.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace MerkaCentro.Web.Controllers;

[Authorize]
public class CashRegisterController : AuthenticatedController
{
    private readonly ICashRegisterService _cashRegisterService;

    public CashRegisterController(ICashRegisterService cashRegisterService)
    {
        _cashRegisterService = cashRegisterService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _cashRegisterService.GetAllAsync(1, 50);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _cashRegisterService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    public IActionResult Open()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(decimal initialCash)
    {
            // user ID comes from authenticated claim; Authorize ensures user is logged in
        var userId = GetCurrentUserId();

        var dto = new OpenCashRegisterDto(userId, initialCash);
        var result = await _cashRegisterService.OpenAsync(dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al abrir la caja");
            return View();
        }

        TempData["Success"] = "Caja abierta exitosamente";
        return RedirectToAction(nameof(Current));
    }

    public async Task<IActionResult> Current()
    {
        var userId = GetCurrentUserId();
        var result = await _cashRegisterService.GetCurrentOpenAsync(userId);

        if (!result.IsSuccess)
        {
            TempData["Warning"] = "No tiene una caja abierta. Abra una caja para continuar.";
            return RedirectToAction(nameof(Open));
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id, decimal countedCash)
    {
        var result = await _cashRegisterService.CloseAsync(id, countedCash);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Caja cerrada exitosamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdrawal(Guid id, decimal amount, string description)
    {
        var dto = new RegisterMovementDto(amount, "COP", description);
        var result = await _cashRegisterService.RegisterWithdrawalAsync(id, dto);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Retiro registrado exitosamente";
        }

        return RedirectToAction(nameof(Current));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(Guid id, decimal amount, string description)
    {
        var dto = new RegisterMovementDto(amount, "COP", description);
        var result = await _cashRegisterService.RegisterDepositAsync(id, dto);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Deposito registrado exitosamente";
        }

        return RedirectToAction(nameof(Current));
    }

    public async Task<IActionResult> Summary(Guid id)
    {
        var result = await _cashRegisterService.GetSummaryAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    // the logic moved to AuthenticatedController base class; keep method for compatibility if needed
}

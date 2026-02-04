using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.DTOs;
using MiniMarket.Application.Services;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Web.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = string.IsNullOrWhiteSpace(search)
            ? await _customerService.GetAllAsync(page, 20)
            : await SearchCustomersAsync(search, page);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.Search = search;
        return View(result.Value);
    }

    private async Task<Application.Common.Result<Application.Common.PagedResult<CustomerDto>>> SearchCustomersAsync(string search, int page)
    {
        var searchResult = await _customerService.SearchAsync(search);
        if (!searchResult.IsSuccess)
        {
            return Application.Common.Result<Application.Common.PagedResult<CustomerDto>>.Failure(searchResult.Error ?? "Error al buscar");
        }

        var customers = searchResult.Value!.ToList();
        var paged = customers.Skip((page - 1) * 20).Take(20);
        return Application.Common.Result<Application.Common.PagedResult<CustomerDto>>.Success(
            new Application.Common.PagedResult<CustomerDto>(paged, customers.Count, page, 20));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _customerService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear el cliente");
            return View(dto);
        }

        TempData["Success"] = "Cliente creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var customer = result.Value!;
        var addressParts = (customer.Address ?? "").Split(',').Select(p => p.Trim()).ToArray();

        var dto = new UpdateCustomerDto(
            customer.Name,
            customer.DocumentNumber,
            customer.DocumentType,
            customer.Phone,
            customer.Email,
            addressParts.Length > 0 ? addressParts[0] : null,
            addressParts.Length > 1 ? addressParts[1] : null,
            addressParts.Length > 2 ? addressParts[2] : null,
            null,
            customer.Notes);

        ViewBag.CustomerId = id;
        ViewBag.CurrentCreditLimit = customer.CreditLimit;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CustomerId = id;
            return View(dto);
        }

        var result = await _customerService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar el cliente");
            ViewBag.CustomerId = id;
            return View(dto);
        }

        TempData["Success"] = "Cliente actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCreditLimit(Guid id, decimal creditLimit)
    {
        var result = await _customerService.SetCreditLimitAsync(id, creditLimit);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Limite de credito actualizado";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _customerService.ActivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Cliente activado";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _customerService.DeactivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Cliente desactivado";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(Guid id, string? reason)
    {
        var result = await _customerService.BlockAsync(id, reason);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Cliente bloqueado";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Cliente eliminado exitosamente";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> WithDebt()
    {
        var result = await _customerService.GetWithDebtAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        return View(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> SearchByDocument(string document)
    {
        var result = await _customerService.GetByDocumentAsync(document);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }
        return Json(result.Value);
    }
}

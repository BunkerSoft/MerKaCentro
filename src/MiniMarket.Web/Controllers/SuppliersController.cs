using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.DTOs;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

public class SuppliersController : Controller
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = string.IsNullOrWhiteSpace(search)
            ? await _supplierService.GetAllAsync(page, 20)
            : await SearchSuppliersAsync(search, page);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.Search = search;
        return View(result.Value);
    }

    private async Task<Application.Common.Result<Application.Common.PagedResult<SupplierDto>>> SearchSuppliersAsync(string search, int page)
    {
        var searchResult = await _supplierService.SearchAsync(search);
        if (!searchResult.IsSuccess)
        {
            return Application.Common.Result<Application.Common.PagedResult<SupplierDto>>.Failure(searchResult.Error ?? "Error al buscar");
        }

        var suppliers = searchResult.Value!.ToList();
        var paged = suppliers.Skip((page - 1) * 20).Take(20);
        return Application.Common.Result<Application.Common.PagedResult<SupplierDto>>.Success(
            new Application.Common.PagedResult<SupplierDto>(paged, suppliers.Count, page, 20));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _supplierService.GetByIdAsync(id);
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
    public async Task<IActionResult> Create(CreateSupplierDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _supplierService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear el proveedor");
            return View(dto);
        }

        TempData["Success"] = "Proveedor creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _supplierService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var supplier = result.Value!;
        var addressParts = (supplier.Address ?? "").Split(',').Select(p => p.Trim()).ToArray();

        var dto = new UpdateSupplierDto(
            supplier.Name,
            supplier.BusinessName,
            supplier.Ruc,
            supplier.Phone,
            supplier.Email,
            addressParts.Length > 0 ? addressParts[0] : null,
            addressParts.Length > 1 ? addressParts[1] : null,
            addressParts.Length > 2 ? addressParts[2] : null,
            supplier.ContactPerson,
            supplier.Notes);

        ViewBag.SupplierId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateSupplierDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.SupplierId = id;
            return View(dto);
        }

        var result = await _supplierService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar el proveedor");
            ViewBag.SupplierId = id;
            return View(dto);
        }

        TempData["Success"] = "Proveedor actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _supplierService.ActivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Proveedor activado";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _supplierService.DeactivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Proveedor desactivado";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _supplierService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Proveedor eliminado exitosamente";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var result = await _supplierService.GetActiveAsync();
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }
        return Json(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> SearchByRuc(string ruc)
    {
        var result = await _supplierService.GetByRucAsync(ruc);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }
        return Json(result.Value);
    }
}

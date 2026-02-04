using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return View(Enumerable.Empty<Application.DTOs.CategoryDto>());
        }
        return View(result.Value);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _categoryService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear la categoría");
            return View(dto);
        }

        TempData["Success"] = "Categoría creada exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdateCategoryDto(result.Value!.Name, result.Value.Description);
        ViewBag.CategoryId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = id;
            return View(dto);
        }

        var result = await _categoryService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar la categoría");
            ViewBag.CategoryId = id;
            return View(dto);
        }

        TempData["Success"] = "Categoría actualizada exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Categoría eliminada exitosamente";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id, bool activate)
    {
        var result = activate
            ? await _categoryService.ActivateAsync(id)
            : await _categoryService.DeactivateAsync(id);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = activate ? "Categoría activada" : "Categoría desactivada";
        }
        return RedirectToAction(nameof(Index));
    }
}

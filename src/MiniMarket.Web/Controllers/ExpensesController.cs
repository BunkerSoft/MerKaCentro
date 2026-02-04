using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniMarket.Application.DTOs;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly IExpenseCategoryService _categoryService;
    private readonly ICashRegisterService _cashRegisterService;

    public ExpensesController(
        IExpenseService expenseService,
        IExpenseCategoryService categoryService,
        ICashRegisterService cashRegisterService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _cashRegisterService = cashRegisterService;
    }

    public async Task<IActionResult> Index(Guid? categoryId, DateTime? from, DateTime? to, int page = 1)
    {
        Application.Common.Result<Application.Common.PagedResult<ExpenseDto>> result;

        if (from.HasValue && to.HasValue)
        {
            var rangeResult = await _expenseService.GetByDateRangeAsync(from.Value, to.Value.AddDays(1));
            if (!rangeResult.IsSuccess)
            {
                TempData["Error"] = rangeResult.Error;
                result = Application.Common.Result<Application.Common.PagedResult<ExpenseDto>>.Failure(rangeResult.Error ?? "Error");
            }
            else
            {
                var expenses = rangeResult.Value!
                    .Where(e => !categoryId.HasValue || e.CategoryId == categoryId.Value)
                    .ToList();
                var paged = expenses.Skip((page - 1) * 20).Take(20);
                result = Application.Common.Result<Application.Common.PagedResult<ExpenseDto>>.Success(
                    new Application.Common.PagedResult<ExpenseDto>(paged, expenses.Count, page, 20));
            }
        }
        else if (categoryId.HasValue)
        {
            var categoryResult = await _expenseService.GetByCategoryAsync(categoryId.Value);
            if (!categoryResult.IsSuccess)
            {
                TempData["Error"] = categoryResult.Error;
                result = Application.Common.Result<Application.Common.PagedResult<ExpenseDto>>.Failure(categoryResult.Error ?? "Error");
            }
            else
            {
                var expenses = categoryResult.Value!.ToList();
                var paged = expenses.Skip((page - 1) * 20).Take(20);
                result = Application.Common.Result<Application.Common.PagedResult<ExpenseDto>>.Success(
                    new Application.Common.PagedResult<ExpenseDto>(paged, expenses.Count, page, 20));
            }
        }
        else
        {
            result = await _expenseService.GetAllAsync(page, 20);
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        await LoadCategoriesDropdownAsync(categoryId);
        ViewBag.SelectedCategory = categoryId;
        ViewBag.FromDate = from?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to?.ToString("yyyy-MM-dd");

        return View(result.Value);
    }

    public async Task<IActionResult> Today()
    {
        var result = await _expenseService.GetTodayExpensesAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        var totalResult = await _expenseService.GetTotalByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(1));
        ViewBag.TotalToday = totalResult.IsSuccess ? totalResult.Value : 0;

        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _expenseService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExpenseDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(dto);
        }

        // TODO: Get actual user ID from authentication
        var userId = Guid.NewGuid();

        var result = await _expenseService.CreateAsync(dto, userId);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al registrar el gasto");
            await LoadDropdownsAsync();
            return View(dto);
        }

        TempData["Success"] = "Gasto registrado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _expenseService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var expense = result.Value!;
        var dto = new UpdateExpenseDto(
            expense.CategoryId,
            expense.Description,
            expense.Amount,
            expense.Reference,
            expense.Notes);

        await LoadCategoriesDropdownAsync(expense.CategoryId);
        ViewBag.ExpenseId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateExpenseDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesDropdownAsync(dto.CategoryId);
            ViewBag.ExpenseId = id;
            return View(dto);
        }

        var result = await _expenseService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar el gasto");
            await LoadCategoriesDropdownAsync(dto.CategoryId);
            ViewBag.ExpenseId = id;
            return View(dto);
        }

        TempData["Success"] = "Gasto actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _expenseService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Gasto eliminado exitosamente";
        }
        return RedirectToAction(nameof(Index));
    }

    // Categories management
    public async Task<IActionResult> Categories()
    {
        var result = await _categoryService.GetAllAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        return View(result.Value);
    }

    public IActionResult CreateCategory()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CreateExpenseCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _categoryService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear la categoria");
            return View(dto);
        }

        TempData["Success"] = "Categoria creada exitosamente";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> EditCategory(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Categories));
        }

        var category = result.Value!;
        var dto = new UpdateExpenseCategoryDto(category.Name, category.Description);

        ViewBag.CategoryId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(Guid id, UpdateExpenseCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = id;
            return View(dto);
        }

        var result = await _categoryService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar la categoria");
            ViewBag.CategoryId = id;
            return View(dto);
        }

        TempData["Success"] = "Categoria actualizada exitosamente";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        var result = await _categoryService.ActivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Categoria activada";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        var result = await _categoryService.DeactivateAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Categoria desactivada";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Categoria eliminada exitosamente";
        }
        return RedirectToAction(nameof(Categories));
    }

    private async Task LoadDropdownsAsync()
    {
        await LoadCategoriesDropdownAsync(null);

        // TODO: Get actual user ID from authentication
        // For now, we'll just check if there's an open register - simplified
        ViewBag.HasOpenCashRegister = false;
        ViewBag.CurrentCashRegisterId = (Guid?)null;
    }

    private async Task LoadCategoriesDropdownAsync(Guid? selectedId)
    {
        var categoriesResult = await _categoryService.GetActiveAsync();
        ViewBag.Categories = categoriesResult.IsSuccess
            ? new SelectList(categoriesResult.Value, "Id", "Name", selectedId)
            : new SelectList(Array.Empty<ExpenseCategoryDto>(), "Id", "Name");
    }
}

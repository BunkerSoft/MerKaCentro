using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniMarket.Application.Services;

namespace MiniMarket.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = string.IsNullOrWhiteSpace(search)
            ? await _productService.GetAllAsync(page, 20)
            : await _productService.SearchAsync(search, page, 20);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.Search = search;
        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View(dto);
        }

        var result = await _productService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear el producto");
            await LoadCategoriesAsync();
            return View(dto);
        }

        TempData["Success"] = "Producto creado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var product = result.Value!;
        var dto = new UpdateProductDto(
            product.Name,
            product.Description,
            product.Barcode,
            product.PurchasePrice,
            product.SalePrice,
            product.MinStock,
            product.Unit,
            product.CategoryId);

        ViewBag.ProductId = id;
        await LoadCategoriesAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            await LoadCategoriesAsync();
            return View(dto);
        }

        var result = await _productService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar el producto");
            ViewBag.ProductId = id;
            await LoadCategoriesAsync();
            return View(dto);
        }

        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Producto eliminado exitosamente";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> LowStock()
    {
        var result = await _productService.GetLowStockAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(Guid id, decimal quantity, string reason)
    {
        var result = await _productService.UpdateStockAsync(id, quantity, reason);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Stock actualizado exitosamente";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> SearchByBarcode(string barcode)
    {
        var result = await _productService.GetByBarcodeAsync(barcode);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }
        return Json(result.Value);
    }

    private async Task LoadCategoriesAsync()
    {
        var categoriesResult = await _categoryService.GetActiveAsync();
        ViewBag.Categories = categoriesResult.IsSuccess
            ? new SelectList(categoriesResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());
    }
}

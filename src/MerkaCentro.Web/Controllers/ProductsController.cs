using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MerkaCentro.Application.Services;

namespace MerkaCentro.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IBarcodeService _barcodeService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        IBarcodeService barcodeService,
        IUnitOfMeasureService unitOfMeasureService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _barcodeService = barcodeService;
        _unitOfMeasureService = unitOfMeasureService;
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

    [HttpGet]
    public async Task<IActionResult> BarcodeImage(Guid id, string? format)
    {
        var product = await _productService.GetByIdAsync(id);
        if (!product.IsSuccess || product.Value is null)
            return NotFound();

        var code = product.Value.Barcode ?? product.Value.Code;
        var barcodeFormat = format?.ToLowerInvariant() switch
        {
            "ean13" => BarcodeFormat.Ean13,
            "ean8" => BarcodeFormat.Ean8,
            "code39" => BarcodeFormat.Code39,
            _ => BarcodeFormat.Code128
        };

        var result = await _barcodeService.GenerateBarcodeAsync(code, barcodeFormat);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return File(result.Value!, "image/bmp");
    }

    [HttpGet]
    public async Task<IActionResult> PrintBarcode(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (!product.IsSuccess || product.Value is null)
        {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        return View(product.Value);
    }

    private async Task LoadCategoriesAsync()
    {
        var categoriesResult = await _categoryService.GetActiveAsync();
        ViewBag.Categories = categoriesResult.IsSuccess
            ? new SelectList(categoriesResult.Value, "Id", "Name")
            : new SelectList(Enumerable.Empty<object>());

        var unitsResult = await _unitOfMeasureService.GetActiveGroupedAsync();
        ViewBag.UnitGroups = unitsResult.IsSuccess
            ? unitsResult.Value!.ToList()
            : new List<UnitOfMeasureGroupDto>();
    }
}

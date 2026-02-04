using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniMarket.Application.DTOs;
using MiniMarket.Application.Services;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Web.Controllers;

public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;

    public PurchaseOrdersController(
        IPurchaseOrderService purchaseOrderService,
        ISupplierService supplierService,
        IProductService productService)
    {
        _purchaseOrderService = purchaseOrderService;
        _supplierService = supplierService;
        _productService = productService;
    }

    public async Task<IActionResult> Index(PurchaseOrderStatus? status, int page = 1)
    {
        Application.Common.PagedResult<PurchaseOrderDto>? pagedResult = null;

        if (status.HasValue)
        {
            var statusResult = await _purchaseOrderService.GetByStatusAsync(status.Value);
            if (!statusResult.IsSuccess)
            {
                TempData["Error"] = statusResult.Error;
            }
            else
            {
                var orders = statusResult.Value!.ToList();
                var pagedOrders = orders.Skip((page - 1) * 20).Take(20).ToList();
                pagedResult = new Application.Common.PagedResult<PurchaseOrderDto>(pagedOrders, orders.Count, page, 20);
            }
        }
        else
        {
            var allResult = await _purchaseOrderService.GetAllAsync(page, 20);
            if (!allResult.IsSuccess)
            {
                TempData["Error"] = allResult.Error;
            }
            else
            {
                pagedResult = allResult.Value;
            }
        }

        ViewBag.SelectedStatus = status;
        ViewBag.Statuses = GetStatusSelectList(status);

        return View(pagedResult);
    }

    public async Task<IActionResult> Pending()
    {
        var result = await _purchaseOrderService.GetPendingAsync();
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id);
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
    public async Task<IActionResult> Create(CreatePurchaseOrderDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(dto);
        }

        // TODO: Get actual user ID from authentication
        var userId = Guid.NewGuid();

        var result = await _purchaseOrderService.CreateAsync(dto, userId);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear la orden de compra");
            await LoadDropdownsAsync();
            return View(dto);
        }

        TempData["Success"] = "Orden de compra creada exitosamente";
        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    public async Task<IActionResult> Receive(Guid id)
    {
        var result = await _purchaseOrderService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var order = result.Value!;
        if (order.Status == PurchaseOrderStatus.Received || order.Status == PurchaseOrderStatus.Cancelled)
        {
            TempData["Error"] = "Esta orden no puede recibir mercancia";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(Guid id, List<ReceiveItemDto> items)
    {
        if (items == null || !items.Any(i => i.ReceivedQuantity > 0))
        {
            TempData["Error"] = "Debe especificar al menos una cantidad a recibir";
            return RedirectToAction(nameof(Receive), new { id });
        }

        var validItems = items.Where(i => i.ReceivedQuantity > 0).ToList();
        var result = await _purchaseOrderService.ReceiveItemsAsync(id, validItems);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Receive), new { id });
        }

        TempData["Success"] = "Mercancia recibida exitosamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsReceived(Guid id)
    {
        var result = await _purchaseOrderService.MarkAsReceivedAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Orden marcada como recibida completamente";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string? reason)
    {
        var result = await _purchaseOrderService.CancelAsync(id, reason);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Orden cancelada";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetBySupplier(Guid supplierId)
    {
        var result = await _purchaseOrderService.GetBySupplierAsync(supplierId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }
        return Json(result.Value);
    }

    private async Task LoadDropdownsAsync()
    {
        var suppliersResult = await _supplierService.GetActiveAsync();
        var productsResult = await _productService.GetActiveAsync();

        ViewBag.Suppliers = suppliersResult.IsSuccess
            ? new SelectList(suppliersResult.Value, "Id", "Name")
            : new SelectList(Array.Empty<SupplierDto>(), "Id", "Name");

        ViewBag.Products = productsResult.IsSuccess
            ? productsResult.Value!.ToList()
            : new List<ProductDto>();
    }

    private static SelectList GetStatusSelectList(PurchaseOrderStatus? selected)
    {
        var statuses = Enum.GetValues<PurchaseOrderStatus>()
            .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) })
            .ToList();

        return new SelectList(statuses, "Value", "Text", selected.HasValue ? (int)selected.Value : null);
    }

    private static string GetStatusDisplayName(PurchaseOrderStatus status)
    {
        return status switch
        {
            PurchaseOrderStatus.Pending => "Pendiente",
            PurchaseOrderStatus.PartiallyReceived => "Parcialmente Recibida",
            PurchaseOrderStatus.Received => "Recibida",
            PurchaseOrderStatus.Cancelled => "Cancelada",
            _ => status.ToString()
        };
    }
}

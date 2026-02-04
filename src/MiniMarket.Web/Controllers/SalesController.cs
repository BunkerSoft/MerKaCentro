using Microsoft.AspNetCore.Mvc;
using MiniMarket.Application.DTOs;
using MiniMarket.Application.Services;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Web.Controllers;

public class SalesController : Controller
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICashRegisterService _cashRegisterService;

    public SalesController(
        ISaleService saleService,
        IProductService productService,
        ICustomerService customerService,
        ICashRegisterService cashRegisterService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;
        _cashRegisterService = cashRegisterService;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int page = 1)
    {
        var result = (from.HasValue && to.HasValue)
            ? await _saleService.GetByDateRangeAsync(from.Value, to.Value, page, 20)
            : await _saleService.GetAllAsync(page, 20);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.FromDate = from?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to?.ToString("yyyy-MM-dd");
        return View(result.Value);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _saleService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }
        return View(result.Value);
    }

    public async Task<IActionResult> Create()
    {
        var userId = GetCurrentUserId();
        var cashRegister = await _cashRegisterService.GetCurrentOpenAsync(userId);

        if (!cashRegister.IsSuccess)
        {
            TempData["Warning"] = "Debe abrir una caja antes de realizar ventas.";
            return RedirectToAction("Open", "CashRegister");
        }

        ViewBag.CashRegisterId = cashRegister.Value!.Id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        var userId = GetCurrentUserId();

        var cashRegister = await _cashRegisterService.GetCurrentOpenAsync(userId);
        if (!cashRegister.IsSuccess)
        {
            return Json(new { success = false, error = "Debe abrir una caja antes de realizar ventas." });
        }

        var items = request.Items.Select(i => new CreateSaleItemDto(
            i.ProductId,
            i.Quantity,
            i.UnitPrice,
            i.DiscountPercent)).ToList();

        var payments = request.Payments.Select(p => new CreateSalePaymentDto(
            Enum.Parse<PaymentMethod>(p.Method),
            p.Amount,
            p.Reference)).ToList();

        var dto = new CreateSaleDto(
            request.CustomerId,
            cashRegister.Value!.Id,
            request.IsCredit,
            request.Notes,
            items,
            payments);

        var result = await _saleService.CreateAsync(dto, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true, saleId = result.Value!.Id, saleNumber = result.Value.Number });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string reason)
    {
        var result = await _saleService.CancelAsync(id, reason);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Venta cancelada exitosamente";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> SearchProduct(string term)
    {
        var result = await _productService.SearchAsync(term, 1, 10);
        if (!result.IsSuccess)
        {
            return Json(new { products = Array.Empty<object>() });
        }

        var products = result.Value!.Items.Select(p => new
        {
            p.Id,
            p.Code,
            p.Barcode,
            p.Name,
            p.SalePrice,
            p.CurrentStock,
            p.Unit
        });

        return Json(new { products });
    }

    [HttpGet]
    public async Task<IActionResult> GetProductByBarcode(string barcode)
    {
        var result = await _productService.GetByBarcodeAsync(barcode);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        var p = result.Value!;
        return Json(new
        {
            p.Id,
            p.Code,
            p.Barcode,
            p.Name,
            p.SalePrice,
            p.CurrentStock,
            p.Unit
        });
    }

    [HttpGet]
    public async Task<IActionResult> SearchCustomer(string term)
    {
        var result = await _customerService.SearchAsync(term);
        if (!result.IsSuccess)
        {
            return Json(new { customers = Array.Empty<object>() });
        }

        var customers = result.Value!
            .Where(c => c.Status == CustomerStatus.Active)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.DocumentNumber,
                c.CreditLimit,
                c.CurrentDebt,
                c.AvailableCredit
            });

        return Json(new { customers });
    }

    public async Task<IActionResult> Today()
    {
        var result = await _saleService.GetTodaySalesAsync();
        var summaryResult = await _saleService.GetDailySummaryAsync(DateTime.Today);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
        }

        ViewBag.Summary = summaryResult.IsSuccess ? summaryResult.Value : null;
        return View(result.Value);
    }

    // TODO: Replace with actual authentication
    private Guid GetCurrentUserId()
    {
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}

public class CreateSaleRequest
{
    public Guid? CustomerId { get; set; }
    public bool IsCredit { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleItemRequest> Items { get; set; } = [];
    public List<CreateSalePaymentRequest> Payments { get; set; } = [];
}

public class CreateSaleItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
}

public class CreateSalePaymentRequest
{
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MerkaCentro.Application.DTOs;
using MerkaCentro.Application.Services;
using MerkaCentro.Domain.Enums;

namespace MerkaCentro.Web.Controllers;

[Authorize]
public class SalesController : AuthenticatedController
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly ITicketPrinterService _ticketPrinterService;
    private readonly ISaleInvoicePdfService _pdfService;
    private readonly IBarcodeService _barcodeService;

    public SalesController(
        ISaleService saleService,
        IProductService productService,
        ICustomerService customerService,
        ICashRegisterService cashRegisterService,
        ITicketPrinterService ticketPrinterService,
        ISaleInvoicePdfService pdfService,
        IBarcodeService barcodeService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;
        _cashRegisterService = cashRegisterService;
        _ticketPrinterService = ticketPrinterService;
        _pdfService = pdfService;
        _barcodeService = barcodeService;
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

        // Print POS ticket automatically (fire-and-forget, don't block the response)
        _ = Task.Run(async () =>
        {
            try { await _ticketPrinterService.PrintSaleTicketAsync(result.Value!); }
            catch { /* Ignore print errors - sale already completed */ }
        });

        return Json(new { success = true, saleId = result.Value!.Id, saleNumber = result.Value.Number });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        var result = await _saleService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        var pdfBytes = await _pdfService.GeneratePdfAsync(result.Value!);
        return File(pdfBytes, "application/pdf", $"Factura_{result.Value!.Number}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrintTicket(Guid id)
    {
        var result = await _saleService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, error = result.Error });
        }

        var printResult = await _ticketPrinterService.PrintSaleTicketAsync(result.Value!);
        return Json(new { success = printResult.IsSuccess, error = printResult.Error });
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

    [HttpPost]
    public async Task<IActionResult> DecodeBarcode()
    {
        if (Request.Form.Files.Count == 0)
            return BadRequest(new { error = "No se envio ninguna imagen." });

        var file = Request.Form.Files[0];
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "La imagen debe pesar entre 1 byte y 5 MB." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        var result = await _barcodeService.DecodeBarcodeAsync(imageBytes);
        if (!result.IsSuccess)
            return Json(new { success = false, error = result.Error });

        return Json(new { success = true, barcode = result.Value });
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

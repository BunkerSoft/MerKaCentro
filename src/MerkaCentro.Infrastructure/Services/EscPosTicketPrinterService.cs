using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MerkaCentro.Application.Common;
using MerkaCentro.Application.DTOs;
using MerkaCentro.Application.Services;
using SkiaSharp;

namespace MerkaCentro.Infrastructure.Services;

public class EscPosTicketPrinterService : ITicketPrinterService
{
    private readonly string _printerName;
    private readonly string _businessName;
    private readonly string _businessNit;
    private readonly string _businessAddress;
    private readonly string _businessPhone;
    private readonly byte[]? _logoEscPosData;
    private readonly ILogger<EscPosTicketPrinterService> _logger;
    private static readonly Encoding _encoding;

    static EscPosTicketPrinterService()
    {
        // Register CodePages provider for .NET 8 (needed for codepage 858)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            _encoding = Encoding.GetEncoding(858);
        }
        catch
        {
            _encoding = Encoding.Latin1;
        }
    }

    // ESC/POS Commands
    private static readonly byte[] ESC_INIT = [0x1B, 0x40];
    private static readonly byte[] ESC_ALIGN_CENTER = [0x1B, 0x61, 0x01];
    private static readonly byte[] ESC_ALIGN_LEFT = [0x1B, 0x61, 0x00];
    private static readonly byte[] ESC_ALIGN_RIGHT = [0x1B, 0x61, 0x02];
    private static readonly byte[] ESC_BOLD_ON = [0x1B, 0x45, 0x01];
    private static readonly byte[] ESC_BOLD_OFF = [0x1B, 0x45, 0x00];
    private static readonly byte[] ESC_DOUBLE_HEIGHT = [0x1B, 0x21, 0x10];
    private static readonly byte[] ESC_NORMAL_SIZE = [0x1B, 0x21, 0x00];
    private static readonly byte[] ESC_CUT_PAPER = [0x1D, 0x56, 0x00];
    private static readonly byte[] ESC_OPEN_DRAWER = [0x1B, 0x70, 0x00, 0x32, 0xC8];
    private static readonly byte[] ESC_FEED_LINES = [0x1B, 0x64, 0x05];

    private const int LINE_WIDTH = 42;

    public EscPosTicketPrinterService(IConfiguration configuration, ILogger<EscPosTicketPrinterService> logger)
    {
        _printerName = configuration["PrinterSettings:PrinterName"] ?? "";
        _businessName = configuration["PrinterSettings:BusinessName"] ?? "MERKACENTRO";
        _businessNit = configuration["PrinterSettings:BusinessNit"] ?? "";
        _businessAddress = configuration["PrinterSettings:BusinessAddress"] ?? "";
        _businessPhone = configuration["PrinterSettings:BusinessPhone"] ?? "";
        _logger = logger;

        var logoPath = configuration["PrinterSettings:LogoPath"];
        if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
        {
            try { _logoEscPosData = ConvertImageToEscPos(logoPath); }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo cargar el logo para ticket"); }
        }
    }

    public Task<Result> PrintSaleTicketAsync(SaleDto sale)
    {
        try
        {
            using var ms = new MemoryStream();

            Write(ms, ESC_INIT);

            // Header
            Write(ms, ESC_ALIGN_CENTER);

            // Logo
            if (_logoEscPosData != null)
            {
                Write(ms, _logoEscPosData);
                WriteText(ms, "");
            }

            Write(ms, ESC_DOUBLE_HEIGHT);
            WriteText(ms, _businessName);
            Write(ms, ESC_NORMAL_SIZE);

            if (!string.IsNullOrWhiteSpace(_businessNit))
                WriteText(ms, $"NIT: {_businessNit}");
            if (!string.IsNullOrWhiteSpace(_businessAddress))
                WriteText(ms, _businessAddress);
            if (!string.IsNullOrWhiteSpace(_businessPhone))
                WriteText(ms, $"Tel: {_businessPhone}");

            WriteText(ms, new string('-', LINE_WIDTH));

            Write(ms, ESC_ALIGN_LEFT);
            WriteText(ms, $"Ticket: {sale.Number}");
            WriteText(ms, $"Fecha: {sale.CreatedAt:dd/MM/yyyy HH:mm}");
            WriteText(ms, $"Vendedor: {sale.UserName}");
            if (!string.IsNullOrWhiteSpace(sale.CustomerName))
                WriteText(ms, $"Cliente: {sale.CustomerName}");

            WriteText(ms, new string('-', LINE_WIDTH));

            // Items
            foreach (var item in sale.Items)
            {
                WriteText(ms, TruncateOrPad(item.ProductName, LINE_WIDTH));
                var detail = $"  {item.Quantity:N2} x $ {item.UnitPrice:N0}";
                var total = $"$ {item.Total:N0}";
                WriteText(ms, PadBetween(detail, total, LINE_WIDTH));
            }

            WriteText(ms, new string('-', LINE_WIDTH));

            // Totals
            Write(ms, ESC_BOLD_ON);
            WriteText(ms, PadBetween("Subtotal:", $"$ {sale.Subtotal:N0}", LINE_WIDTH));
            if (sale.Discount > 0)
                WriteText(ms, PadBetween("Descuento:", $"$ {sale.Discount:N0}", LINE_WIDTH));
            if (sale.Tax > 0)
                WriteText(ms, PadBetween("IVA:", $"$ {sale.Tax:N0}", LINE_WIDTH));
            Write(ms, ESC_DOUBLE_HEIGHT);
            WriteText(ms, PadBetween("TOTAL:", $"$ {sale.Total:N0}", LINE_WIDTH));
            Write(ms, ESC_NORMAL_SIZE);
            Write(ms, ESC_BOLD_OFF);

            WriteText(ms, new string('-', LINE_WIDTH));

            WriteText(ms, PadBetween("Pagado:", $"$ {sale.AmountPaid:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Cambio:", $"$ {sale.Change:N0}", LINE_WIDTH));

            // Footer
            WriteText(ms, new string('=', LINE_WIDTH));
            Write(ms, ESC_ALIGN_CENTER);

            // QR Code (ESC/POS native commands)
            var qrContent = $"{_businessName}|NIT:{_businessNit}|{sale.Number}|{sale.CreatedAt:yyyy-MM-dd HH:mm}|Total:${sale.Total:N0}";
            WriteQrCode(ms, qrContent);

            WriteText(ms, "Gracias por su compra!");
            WriteText(ms, "");

            Write(ms, ESC_FEED_LINES);
            Write(ms, ESC_CUT_PAPER);

            var bytes = ms.ToArray();

            if (string.IsNullOrWhiteSpace(_printerName))
            {
                _logger.LogWarning("Nombre de impresora no configurado. Ticket no enviado.");
                return Task.FromResult(Result.Failure("Impresora no configurada. Configure 'PrinterSettings:PrinterName' en appsettings.json"));
            }

            var sent = RawPrinterHelper.SendBytesToPrinter(_printerName, bytes);
            if (!sent)
            {
                _logger.LogError("No se pudo enviar el ticket a la impresora '{PrinterName}'", _printerName);
                return Task.FromResult(Result.Failure($"No se pudo imprimir en '{_printerName}'. Verifique que la impresora este conectada y encendida."));
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir ticket de venta");
            return Task.FromResult(Result.Failure($"Error de impresion: {ex.Message}"));
        }
    }

    public Task<Result> PrintCashClosingAsync(CashRegisterDto cashRegister)
    {
        try
        {
            using var ms = new MemoryStream();

            Write(ms, ESC_INIT);
            Write(ms, ESC_ALIGN_CENTER);
            Write(ms, ESC_DOUBLE_HEIGHT);
            WriteText(ms, "CIERRE DE CAJA");
            Write(ms, ESC_NORMAL_SIZE);
            WriteText(ms, _businessName);
            WriteText(ms, new string('=', LINE_WIDTH));

            Write(ms, ESC_ALIGN_LEFT);
            WriteText(ms, $"Usuario: {cashRegister.UserName}");
            WriteText(ms, $"Apertura: {cashRegister.OpenedAt:dd/MM/yyyy HH:mm}");
            if (cashRegister.ClosedAt.HasValue)
                WriteText(ms, $"Cierre: {cashRegister.ClosedAt:dd/MM/yyyy HH:mm}");
            WriteText(ms, new string('-', LINE_WIDTH));

            Write(ms, ESC_BOLD_ON);
            WriteText(ms, PadBetween("Inicial:", $"$ {cashRegister.InitialCash:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Ventas:", $"$ {cashRegister.Summary.TotalSales:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Gastos:", $"$ {cashRegister.Summary.TotalExpenses:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Retiros:", $"$ {cashRegister.Summary.TotalWithdrawals:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Depositos:", $"$ {cashRegister.Summary.TotalDeposits:N0}", LINE_WIDTH));
            WriteText(ms, new string('-', LINE_WIDTH));
            WriteText(ms, PadBetween("Esperado:", $"$ {cashRegister.ExpectedCash:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Contado:", $"$ {cashRegister.FinalCash:N0}", LINE_WIDTH));
            WriteText(ms, PadBetween("Diferencia:", $"$ {cashRegister.Difference:N0}", LINE_WIDTH));
            Write(ms, ESC_BOLD_OFF);

            WriteText(ms, new string('=', LINE_WIDTH));
            Write(ms, ESC_FEED_LINES);
            Write(ms, ESC_CUT_PAPER);

            var bytes = ms.ToArray();

            if (string.IsNullOrWhiteSpace(_printerName))
                return Task.FromResult(Result.Failure("Impresora no configurada."));

            var sent = RawPrinterHelper.SendBytesToPrinter(_printerName, bytes);
            return Task.FromResult(sent
                ? Result.Success()
                : Result.Failure("No se pudo imprimir el cierre de caja."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir cierre de caja");
            return Task.FromResult(Result.Failure($"Error de impresion: {ex.Message}"));
        }
    }

    public Task<Result> PrintReportAsync(string title, IEnumerable<string> lines)
    {
        try
        {
            using var ms = new MemoryStream();

            Write(ms, ESC_INIT);
            Write(ms, ESC_ALIGN_CENTER);
            Write(ms, ESC_BOLD_ON);
            WriteText(ms, title.ToUpperInvariant());
            Write(ms, ESC_BOLD_OFF);
            WriteText(ms, new string('=', LINE_WIDTH));

            Write(ms, ESC_ALIGN_LEFT);
            foreach (var line in lines)
                WriteText(ms, line);

            WriteText(ms, new string('=', LINE_WIDTH));
            Write(ms, ESC_FEED_LINES);
            Write(ms, ESC_CUT_PAPER);

            var bytes = ms.ToArray();

            if (string.IsNullOrWhiteSpace(_printerName))
                return Task.FromResult(Result.Failure("Impresora no configurada."));

            var sent = RawPrinterHelper.SendBytesToPrinter(_printerName, bytes);
            return Task.FromResult(sent ? Result.Success() : Result.Failure("No se pudo imprimir el reporte."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir reporte");
            return Task.FromResult(Result.Failure($"Error de impresion: {ex.Message}"));
        }
    }

    public Task<Result> OpenCashDrawerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_printerName))
                return Task.FromResult(Result.Failure("Impresora no configurada."));

            var sent = RawPrinterHelper.SendBytesToPrinter(_printerName, ESC_OPEN_DRAWER);
            return Task.FromResult(sent ? Result.Success() : Result.Failure("No se pudo abrir la caja."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir cajon");
            return Task.FromResult(Result.Failure($"Error: {ex.Message}"));
        }
    }

    public Task<Result<bool>> IsAvailableAsync()
    {
        var available = !string.IsNullOrWhiteSpace(_printerName);
        return Task.FromResult(Result<bool>.Success(available));
    }

    private static void Write(MemoryStream ms, byte[] data) => ms.Write(data, 0, data.Length);

    private static void WriteText(MemoryStream ms, string text)
    {
        var bytes = _encoding.GetBytes(text + "\n");
        ms.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Imprime un QR code usando comandos nativos ESC/POS (GS ( k).
    /// Compatible con la mayoria de impresoras termicas modernas.
    /// </summary>
    private static void WriteQrCode(MemoryStream ms, string content)
    {
        var data = _encoding.GetBytes(content);
        int len = data.Length + 3;
        int pL = len % 256;
        int pH = len / 256;

        // GS ( k - QR Code: Select model (Model 2)
        Write(ms, [0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00]);

        // GS ( k - QR Code: Set module size (4 dots)
        Write(ms, [0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x04]);

        // GS ( k - QR Code: Set error correction level (M = 49)
        Write(ms, [0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31]);

        // GS ( k - QR Code: Store data
        Write(ms, [0x1D, 0x28, 0x6B, (byte)pL, (byte)pH, 0x31, 0x50, 0x30]);
        Write(ms, data);

        // GS ( k - QR Code: Print
        Write(ms, [0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30]);

        WriteText(ms, ""); // line feed after QR
    }

    /// <summary>
    /// Convierte una imagen (JPEG/PNG) a formato ESC/POS raster bitmap
    /// usando comandos GS v 0 (raster bit image).
    /// Escala la imagen a un ancho maximo de 384 px (impresora 80mm).
    /// </summary>
    private static byte[] ConvertImageToEscPos(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        if (bitmap == null) return [];

        // Escalar al ancho del ticket (max 384 px para impresora 80mm)
        const int maxWidth = 384;
        int newWidth = Math.Min(bitmap.Width, maxWidth);
        int newHeight = (int)((double)bitmap.Height / bitmap.Width * newWidth);

        using var scaled = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
        if (scaled == null) return [];

        // El ancho en bytes debe ser multiplo de 8
        int widthBytes = (scaled.Width + 7) / 8;
        int height = scaled.Height;

        using var ms = new MemoryStream();

        // GS v 0 - Print raster bit image
        // m=0 (normal), xL/xH = width bytes, yL/yH = height
        ms.WriteByte(0x1D); // GS
        ms.WriteByte(0x76); // v
        ms.WriteByte(0x30); // 0
        ms.WriteByte(0x00); // m = normal
        ms.WriteByte((byte)(widthBytes % 256));   // xL
        ms.WriteByte((byte)(widthBytes / 256));    // xH
        ms.WriteByte((byte)(height % 256));        // yL
        ms.WriteByte((byte)(height / 256));         // yH

        // Pixel data: 1 bit = black, 0 = white
        for (int y = 0; y < height; y++)
        {
            for (int byteX = 0; byteX < widthBytes; byteX++)
            {
                byte b = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    int x = byteX * 8 + bit;
                    if (x < scaled.Width)
                    {
                        var pixel = scaled.GetPixel(x, y);
                        // Convertir a escala de grises y umbral
                        int gray = (pixel.Red * 299 + pixel.Green * 587 + pixel.Blue * 114) / 1000;
                        if (gray < 128) // negro
                            b |= (byte)(0x80 >> bit);
                    }
                }
                ms.WriteByte(b);
            }
        }

        return ms.ToArray();
    }

    private static string TruncateOrPad(string text, int width)
    {
        return text.Length > width ? text[..width] : text;
    }

    private static string PadBetween(string left, string right, int width)
    {
        var spaces = width - left.Length - right.Length;
        if (spaces < 1) spaces = 1;
        return left + new string(' ', spaces) + right;
    }
}

using Microsoft.Extensions.Configuration;
using MerkaCentro.Application.DTOs;
using MerkaCentro.Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MerkaCentro.Infrastructure;

public class SaleInvoicePdfService : ISaleInvoicePdfService
{
    private readonly string _businessName;
    private readonly string _businessNit;
    private readonly string _businessAddress;
    private readonly string _businessPhone;
    private readonly byte[]? _logoBytes;

    public SaleInvoicePdfService(IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _businessName = configuration["PrinterSettings:BusinessName"] ?? "MERKACENTRO";
        _businessNit = configuration["PrinterSettings:BusinessNit"] ?? "";
        _businessAddress = configuration["PrinterSettings:BusinessAddress"] ?? "";
        _businessPhone = configuration["PrinterSettings:BusinessPhone"] ?? "";

        var logoPath = configuration["PrinterSettings:LogoPath"];
        if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
        {
            _logoBytes = File.ReadAllBytes(logoPath);
        }
    }

    public Task<byte[]> GeneratePdfAsync(SaleDto sale)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, sale));
                page.Content().Element(c => ComposeContent(c, sale));
                page.Footer().Element(c => ComposeFooter(c, sale));
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private void ComposeHeader(IContainer container, SaleDto sale)
    {
        container.Column(col =>
        {
            // Logo + Nombre del negocio
            if (_logoBytes != null)
            {
                col.Item().AlignCenter().Width(80).Image(_logoBytes);
                col.Item().PaddingTop(5);
            }

            col.Item().AlignCenter().Text(_businessName).Bold().FontSize(16);

            if (!string.IsNullOrWhiteSpace(_businessNit))
                col.Item().AlignCenter().Text($"NIT: {_businessNit}").FontSize(9);

            if (!string.IsNullOrWhiteSpace(_businessAddress))
                col.Item().AlignCenter().Text(_businessAddress).FontSize(9);

            if (!string.IsNullOrWhiteSpace(_businessPhone))
                col.Item().AlignCenter().Text($"Tel: {_businessPhone}").FontSize(9);

            col.Item().PaddingVertical(5).LineHorizontal(1);

            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text($"Factura: {sale.Number}").Bold();
                    left.Item().Text($"Fecha: {sale.CreatedAt:dd/MM/yyyy HH:mm}");
                });

                if (!string.IsNullOrWhiteSpace(sale.CustomerName))
                {
                    row.RelativeItem().Column(right =>
                    {
                        right.Item().Text($"Cliente: {sale.CustomerName}");
                    });
                }
            });

            col.Item().PaddingVertical(5).LineHorizontal(1);
        });
    }

    private static void ComposeContent(IContainer container, SaleDto sale)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Producto").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Cant.").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("P. Unit.").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Subtotal").Bold();
                });

                foreach (var item in sale.Items)
                {
                    table.Cell().Padding(4).Text(item.ProductName);
                    table.Cell().Padding(4).AlignRight().Text($"{item.Quantity:N2}");
                    table.Cell().Padding(4).AlignRight().Text($"$ {item.UnitPrice:N0}");
                    table.Cell().Padding(4).AlignRight().Text($"$ {item.Total:N0}");
                }
            });

            col.Item().PaddingVertical(10).LineHorizontal(1);

            col.Item().AlignRight().Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Subtotal:");
                    row.ConstantItem(120).AlignRight().Text($"$ {sale.Subtotal:N0}");
                });

                if (sale.Discount > 0)
                {
                    totals.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text("Descuento:");
                        row.ConstantItem(120).AlignRight().Text($"$ {sale.Discount:N0}");
                    });
                }

                if (sale.Tax > 0)
                {
                    totals.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text("IVA:");
                        row.ConstantItem(120).AlignRight().Text($"$ {sale.Tax:N0}");
                    });
                }

                totals.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("TOTAL:").Bold().FontSize(12);
                    row.ConstantItem(120).AlignRight().Text($"$ {sale.Total:N0}").Bold().FontSize(12);
                });
            });

            col.Item().PaddingVertical(10).LineHorizontal(1);

            col.Item().Column(payInfo =>
            {
                payInfo.Item().Text($"Metodo de pago: {sale.PaymentMethod}");
                payInfo.Item().Text($"Monto pagado: $ {sale.AmountPaid:N0}");

                if (sale.Change > 0)
                    payInfo.Item().Text($"Cambio: $ {sale.Change:N0}");
            });

            if (sale.Payments.Count > 1)
            {
                col.Item().PaddingTop(10).Column(paymentsCol =>
                {
                    paymentsCol.Item().Text("Detalle de pagos:").Bold();
                    foreach (var payment in sale.Payments)
                    {
                        var text = $"  {payment.Method}: $ {payment.Amount:N0}";
                        if (!string.IsNullOrWhiteSpace(payment.Reference))
                            text += $" (Ref: {payment.Reference})";
                        paymentsCol.Item().Text(text);
                    }
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, SaleDto sale)
    {
        container.Column(col =>
        {
            col.Item().PaddingVertical(10).LineHorizontal(1);

            // QR Code
            var qrContent = BuildQrContent(sale);
            try
            {
                var qrBytes = GenerateQrBmp(qrContent);
                if (qrBytes.Length > 0)
                {
                    col.Item().AlignCenter().Width(120).Height(120).Image(qrBytes);
                }
            }
            catch
            {
                // Si falla el QR, continuar sin el
            }

            col.Item().AlignCenter().Text("Gracias por su compra").Italic();
            col.Item().AlignCenter().Text(text =>
            {
                text.Span("Pagina ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private string BuildQrContent(SaleDto sale)
    {
        return $"{_businessName}|NIT:{_businessNit}|{sale.Number}|{sale.CreatedAt:yyyy-MM-dd HH:mm}|Total:${sale.Total:N0}";
    }

    private static byte[] GenerateQrBmp(string content)
    {
        var matrix = Application.Services.BarcodeService.GenerateQrMatrix(content, 200);
        int w = matrix.Width, h = matrix.Height;
        int rowSize = (w * 3 + 3) & ~3;
        int pixelDataSize = rowSize * h;
        int fileSize = 54 + pixelDataSize;
        var bmp = new byte[fileSize];

        bmp[0] = 0x42; bmp[1] = 0x4D;
        BitConverter.GetBytes(fileSize).CopyTo(bmp, 2);
        BitConverter.GetBytes(54).CopyTo(bmp, 10);
        BitConverter.GetBytes(40).CopyTo(bmp, 14);
        BitConverter.GetBytes(w).CopyTo(bmp, 18);
        BitConverter.GetBytes(h).CopyTo(bmp, 22);
        BitConverter.GetBytes((short)1).CopyTo(bmp, 26);
        BitConverter.GetBytes((short)24).CopyTo(bmp, 28);
        BitConverter.GetBytes(pixelDataSize).CopyTo(bmp, 34);

        for (int y = 0; y < h; y++)
        {
            int dstRow = 54 + y * rowSize;
            for (int x = 0; x < w; x++)
            {
                int dstIdx = dstRow + x * 3;
                byte color = matrix[x, h - 1 - y] ? (byte)0 : (byte)255;
                bmp[dstIdx] = color;
                bmp[dstIdx + 1] = color;
                bmp[dstIdx + 2] = color;
            }
        }
        return bmp;
    }
}

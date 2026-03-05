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

    public SaleInvoicePdfService(IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _businessName = configuration["PrinterSettings:BusinessName"] ?? "MERKACENTRO";
        _businessNit = configuration["PrinterSettings:BusinessNit"] ?? "";
        _businessAddress = configuration["PrinterSettings:BusinessAddress"] ?? "";
        _businessPhone = configuration["PrinterSettings:BusinessPhone"] ?? "";
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
                page.Footer().Element(ComposeFooter);
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private void ComposeHeader(IContainer container, SaleDto sale)
    {
        container.Column(col =>
        {
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

    private static void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingVertical(10).LineHorizontal(1);
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
}
